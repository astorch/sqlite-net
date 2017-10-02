using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif

namespace SQLite.Tests
{
	[TestFixture]
	public class TypeConverterTest
	{
		class CompositeId
		{
			public string Fragment1 { get; set; }

			public string Fragment2 { get; set; }

		}

		class ComplexType
		{
			[PrimaryKey]
			public CompositeId Id { get; set; }

			public string Value { get; set; }
		}

		class CompositeIdTypeConverter : ITypeConverter
		{
			/// <inheritdoc />
			public object ToDatabaseValue (object modelObject)
			{
				CompositeId id = (CompositeId) modelObject;
				return $"{id.Fragment1}.{id.Fragment2}";
			}

			/// <inheritdoc />
			public object FromDatabaseValue (object databaseValue)
			{
				string value = (string) databaseValue;
				string[] parts = value.Split ('.');
				return new CompositeId {
					Fragment1 = parts[0],
					Fragment2 = parts[1]
				};
			}
		}

		class TableMappingInfoProvider : ITableMappingInfoProvider
		{
			/// <inheritdoc />
			public ITableMappingInfo GetTableMapping (Type type)
			{
				if (type == typeof(ComplexType)) return new TableMapping();

				return new ReflectiveTableMappingInfoProvider().GetTableMapping(type);
			}

			class TableMapping : ITableMappingInfo
			{
				/// <inheritdoc />
				public Type ClassType { get; } = typeof(ComplexType);

				/// <inheritdoc />
				public string TableName { get; } = "complex_type";

				/// <inheritdoc />
				public bool WithoutRowId { get; } = false;

				/// <inheritdoc />
				public ICollection<IColumnMappingInfo> Columns { get; } = new List<IColumnMappingInfo> () {
					new ColumnMapping {Name = "id", Property = typeof(ComplexType).GetProperty("Id"), ColumnType = typeof(string), IsPk = true, IsAutoInc = false, NotNull = true},
					new ColumnMapping {Name = "value", Property = typeof(ComplexType).GetProperty("Value"), ColumnType = typeof(string), IsPk = false, IsAutoInc = false, NotNull = false},
				};
			}

			class ColumnMapping : IColumnMappingInfo
			{
				/// <inheritdoc />
				public string Name { get; set; }

				/// <inheritdoc />
				public PropertyInfo Property { get; set; }

				/// <inheritdoc />
				public string Collation { get; set; }

				/// <inheritdoc />
				public Type ColumnType { get; set; }

				/// <inheritdoc />
				public bool IsPk { get; set; }

				/// <inheritdoc />
				public bool IsAutoInc { get; set; }

				/// <inheritdoc />
				public IEnumerable<IndexedAttribute> Indices { get; } = new IndexedAttribute[0];

				/// <inheritdoc />
				public bool NotNull { get; set; }

				/// <inheritdoc />
				public int? MaxStringLength { get; set; }

				/// <inheritdoc />
				public bool StoreAsText { get; set; }

				/// <inheritdoc />
				public IPrimaryKeyGenerator PrimaryKeyGenerator { get; } = null;
			}
		}

		[Test]
		public void InsertEntry ()
		{
			using (var db = new TestDb ()) {
				// Arrange
				SQLiteConnection.Configuration.TableMappingInfoProvider = new TableMappingInfoProvider();
				SQLiteConnection.Configuration.CustomTypeConverter.Add(typeof(CompositeId), new CompositeIdTypeConverter());

				// Act
				db.CreateTable<ComplexType> ();
				db.Insert (new ComplexType {
					Id = new CompositeId {
						Fragment1 = "a",
						Fragment2 = "b"
					},
					Value = "none"
				});

				var r = db.Table<ComplexType> ().First ();

				// Assert
				Assert.NotNull(r);
				Assert.NotNull(r.Id);
				Assert.AreEqual("a", r.Id.Fragment1);
				Assert.AreEqual("b", r.Id.Fragment2);
				Assert.AreEqual("none", r.Value);
			}
		}
	}
}

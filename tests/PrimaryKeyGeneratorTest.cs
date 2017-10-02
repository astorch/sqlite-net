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
	public class PrimaryKeyGeneratorTest
	{
		class MySpecialObject
		{
			[PrimaryKey, PrimaryKeyGenerator(typeof(MyPkGenerator))]
			public int Id { get; set; }

			public string Text { get; set; }
		}

		class MyPkGenerator : IPrimaryKeyGenerator
		{
			/// <inheritdoc />
			public object GeneratePrimaryKey (object clrObject, int rowCount)
			{
				return rowCount ^ 2;
			}
		}

		[Test]
		public void InsertWithPrimaryKeyGenerator ()
		{
			using (var db = new TestDb ()) {
				// Arrange
				db.CreateTable<MySpecialObject> ();
				
				// Act
				db.Insert (new MySpecialObject { Text = "a" });
				db.Insert (new MySpecialObject { Text = "b" });

				// Assert
				var recordSet = db.Table<MySpecialObject> ().ToList();
				Assert.AreEqual(2, recordSet.Count);

				MySpecialObject o1 = recordSet[0];
				Assert.NotNull(o1);
				Assert.AreEqual(2, o1.Id);

				MySpecialObject o2 = recordSet[1];
				Assert.NotNull(o2);
				Assert.AreEqual (3, o2.Id);
			}
		}
	}
}

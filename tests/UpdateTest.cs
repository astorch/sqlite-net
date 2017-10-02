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
	public class UpdateTest
	{
		class MyUpdatable
		{
			[PrimaryKey, AutoIncrement]
			public int Id { get; set; }

			public string Text1 { get; set; }

			public string Text2 { get; set; }
		}

		[Test]
		public void UpdateWithPropertySet ()
		{
			using (var db = new TestDb ()) {
				// Arrange
				db.CreateTable<MyUpdatable> ();
				db.Insert (new MyUpdatable { Text1 = "Message a" });

				// Act
				var entity = db.Table<MyUpdatable> ().First ();
				entity.Text2 = "updated message";
				int updateCount = db.Update (entity, new[] {nameof(MyUpdatable.Text2)});
				var finalEntity = db.Table<MyUpdatable> ().First ();

				// Assert
				Assert.AreEqual(1, updateCount);
				Assert.NotNull(finalEntity);
				Assert.AreEqual("Message a", finalEntity.Text1);
				Assert.AreEqual ("updated message", finalEntity.Text2);
			}
		}
	}
}

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MealCardCollection.Util;
namespace UnitTestProject1
{
    /// <summary>
    /// TestReadini 的摘要说明
    /// 对MealCardCollection进行测试
    /// </summary>
    [TestClass]
    public class TestReadini
    {
        public TestReadini()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO:  在此处添加测试逻辑
            //
        }
        [TestMethod]
        public void TestWriteIniFile()
        {
            
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            Console.WriteLine(path);
            string section = "LocalConfig";
            string section2 = "RemoteConfig";
            string key = "Server";
            string key1 = "Database";
            string key2 = "Username";
            string key3 = "Password";
            string key4 = "Machport";
            long flag= Readini.WriteIniFile(section,key,"testvalue",path);
            Console.WriteLine(flag);
            Assert.AreEqual(0,flag);
        }
        [TestMethod]
        public void TestReadIniFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string section = "LocalConfig";
            string key = "Server";
            string value = Readini.ReadIniFile(section,key,path);
            Console.WriteLine(value);
            Assert.AreEqual("testvalue",value);
        }
    }
}

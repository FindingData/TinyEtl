using NUnit.Framework;
using System;
using System.IO;

namespace FD.TinyEtl.UTest
{
    [TestFixture]
    public class StructureCheckTest
    {        


        [Test]
        public void FileNotExistTest()
        {
            var check = new ExcelCheck(null);
            //check._checkReport.CheckErrorList
            
        }

        [Test]
        public void FileNotExistTest2()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = path + "\\SimpleTemplate\\征收外勘情况录入表.xlsx";
            using (FileStream file = new FileStream("test.xls", FileMode.Create))
            {
                var check = new ExcelCheck(file);

            }
            //var stream = File.OpenRead("");

        }
    }
}

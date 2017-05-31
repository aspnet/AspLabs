using System;
using Xunit;
using MyMvcApp;

namespace MyMvcApp.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal("pass", MyMvcApp.Program.Test);
        }
    }
}

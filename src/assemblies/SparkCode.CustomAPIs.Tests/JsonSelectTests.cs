using System;
using Xunit;
using SparkCode.CustomAPIs;

namespace SparkCode.CustomAPIs.Tests
{
    public class JsonSelectTests
    {
        private string TestInput1 = @"{
                  'Stores': [
                    'Lambton Quay',
                    'Willis Street'
                  ],
                  'Manufacturers': [
                    {
                      'Name': 'Acme Co',
                      'Products': [
                        {
                          'Name': 'Anvil',
                          'Price': 50
                        }
                      ]
                    },
                    {
                      'Name': 'Contoso',
                      'Products': [
                        {
                          'Name': 'Elbow Grease',
                          'Price': 99.95
                        },
                        {
                          'Name': 'Headlight Fluid',
                          'Price': 4
                        }
                      ]
                    }
                  ]
                }";

        private string TestInput2 = @"{ 'store': {
                    'book': [
                      { 'category': 'reference',
                        'author': 'Nigel Rees',
                        'title': 'Sayings of the Century',
                        'price': 8.95
                      },
                      { 'category': 'fiction',
                        'author': 'Evelyn Waugh',
                        'title': 'Sword of Honour',
                        'price': 12.99
                      },
                    ],
                    'bicycle': {
                      'color': 'red',
                      'price': 399
                    }
                  }
                }";

        [Fact]
        public void SimpleSingleQuery()
        {
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, "{\"name\":\"John\", \"age\":30}", "$.name");
            Assert.Equal("John", output);
        }

        [Fact]
        public void SingleQueryReturningObject()
        {
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, TestInput2, "$..book[0]");
            var expected = @"{
  ""category"": ""reference"",
  ""author"": ""Nigel Rees"",
  ""title"": ""Sayings of the Century"",
  ""price"": 8.95
}";
            Assert.Equal(expected, output);
        }

        [Fact]
        public void SingleQueryReturningNothing() 
        { 
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, TestInput2, "$.store.book[5]");
            Assert.Null(output); // Should return null if no match found
        }

        // Test Multiple query returning nothing
        [Fact]
        public void MultipleQueryReturningNothing()
        {
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, TestInput1, "$..abc");
            Assert.Null(output); // Should return null if no match found
        }


        [Fact]
        public void MultipleQuery()
        {
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, TestInput1, "$..Products[?(@.Price >= 50)].Name");
            Assert.Equal("Anvil,Elbow Grease", output);
        }

        [Fact]
        public void MultipleQueryReturningTwoLargeDifferentObjects()
        {
            JSonSelect jsonSelect = new JSonSelect();
            var ctx = new Context();
            var output = jsonSelect.Select(ctx, TestInput2, "$.store.*");
            var expected = @"[
  {
    ""category"": ""reference"",
    ""author"": ""Nigel Rees"",
    ""title"": ""Sayings of the Century"",
    ""price"": 8.95
  },
  {
    ""category"": ""fiction"",
    ""author"": ""Evelyn Waugh"",
    ""title"": ""Sword of Honour"",
    ""price"": 12.99
  }
],{
  ""color"": ""red"",
  ""price"": 399
}";
            Assert.Equal(expected,output);

        }
    }
}

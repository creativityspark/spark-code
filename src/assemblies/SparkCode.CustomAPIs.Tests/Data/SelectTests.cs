using SparkCode.CustomAPIs.Data;
using System;
using Xunit;

namespace SparkCode.CustomAPIs.Tests.Data
{
    public class SelectTests
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
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery("{\"name\":\"John\", \"age\":30}", "$.name");
            Assert.Equal("John", output);
        }

        [Fact]
        public void SingleQueryReturningObject()
        {
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery(TestInput2, "$..book[0]");
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
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery(TestInput2, "$.store.book[5]");
            Assert.Null(output); // Should return null if no match found
        }

        // Test Multiple query returning nothing
        [Fact]
        public void MultipleQueryReturningNothing()
        {
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery(TestInput1, "$..abc");
            Assert.Null(output); // Should return null if no match found
        }


        [Fact]
        public void MultipleQuery()
        {
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery(TestInput1, "$..Products[?(@.Price >= 50)].Name");
            Assert.Equal("Anvil,Elbow Grease", output);
        }

        [Fact]
        public void MultipleQueryReturningTwoLargeDifferentObjects()
        {
            Select jsonSelect = new Select(new Context());
            var output = jsonSelect.RunQuery(TestInput2, "$.store.*");
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
            Assert.Equal(expected, output);

        }
    }
}

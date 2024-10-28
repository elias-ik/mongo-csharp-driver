﻿/* Copyright 2016-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq.Linq3Implementation;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToExecutableQueryTranslators;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationWithLinq2Tests
{
    public class MongoQueryableEnumComparedToEnumWithStringRepresentationTests
    {
        private static readonly IMongoClient __client;
        private static readonly IMongoCollection<C> __collection;
        private static readonly IMongoDatabase __database;

        static MongoQueryableEnumComparedToEnumWithStringRepresentationTests()
        {
            __client = DriverTestConfiguration.Client;
            __database = __client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            __collection = __database.GetCollection<C>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }

        public enum E { A, B };

        public class C
        {
            [BsonRepresentation(BsonType.String)]
            public E E { get; set; }
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : \"A\" }")]
        [InlineData(E.B, "{ \"E\" : \"B\" }")]
        public void Where_operator_equal_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E == value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : { \"$gt\" : \"A\" } }")]
        [InlineData(E.B, "{ \"E\" : { \"$gt\" : \"B\" } }")]
        public void Where_operator_greater_than_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E > value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : { \"$gte\" : \"A\" } }")]
        [InlineData(E.B, "{ \"E\" : { \"$gte\" : \"B\" } }")]
        public void Where_operator_greater_than_or_equal_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E >= value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : { \"$lt\" : \"A\" } }")]
        [InlineData(E.B, "{ \"E\" : { \"$lt\" : \"B\" } }")]
        public void Where_operator_less_than_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E < value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : { \"$lte\" : \"A\" } }")]
        [InlineData(E.B, "{ \"E\" : { \"$lte\" : \"B\" } }")]
        public void Where_operator_less_than_or_equal_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E <= value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : { \"$ne\" : \"A\" } }")]
        [InlineData(E.B, "{ \"E\" : { \"$ne\" : \"B\" } }")]
        public void Where_operator_not_equal_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable();

            var queryable = subject.Where(x => x.E != value);

            AssertFilter(queryable, expectedFilter);
        }

        // private methods
        private void AssertFilter<T>(IQueryable<T> queryable, string expectedFilter)
        {
            var stages = Translate(queryable);
            stages.Should().HaveCount(1);
            stages[0].Should().Be($"{{ \"$match\" : {expectedFilter} }}");
        }

        private BsonDocument[] Translate<T>(IQueryable<T> queryable)
        {
            var provider = (MongoQueryProvider<T>)queryable.Provider;
            var executableQuery = ExpressionToExecutableQueryTranslator.Translate<T, T>(provider, queryable.Expression, translationOptions: null);
            return executableQuery.Pipeline.Stages.Select(s => (BsonDocument)s.Render()).ToArray();
        }
    }
}

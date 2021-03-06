//-----------------------------------------------------------------------
// <copyright file="DeleteIndexes.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Raven.Abstractions.Indexing;
using Raven.Client.Embedded;
using Raven.Database;
using Raven.Database.Config;
using Raven.Tests.Common;

using Xunit;
using System.Linq;

namespace Raven.Tests.Storage
{
    public class DeleteIndexes : RavenTest
    {
        private readonly EmbeddableDocumentStore store;
        private readonly DocumentDatabase db;

        public DeleteIndexes()
        {
            store = NewDocumentStore();
            db = store.SystemDatabase;
        }

        public override void Dispose()
        {
            store.Dispose();
            base.Dispose();
        }

        [Fact]
        public void Can_remove_index()
        {
            db.Indexes.PutIndex("pagesByTitle",
                       new IndexDefinition
                       {
                           Map = @"
    from doc in docs
    where doc.type == ""page""
    select new { Key = doc.title, Value = doc.content, Size = doc.size };
"
                       });
            db.Indexes.DeleteIndex("pagesByTitle");
            var indexNames = db.IndexDefinitionStorage.IndexNames.Where(x => x.StartsWith("Raven") == false).ToArray();
            Assert.Equal(0, indexNames.Length);
        }

        [Fact]
        public void Removing_index_remove_it_from_index_storage()
        {
            const string definition =
                @"
    from doc in docs
    where doc.type == ""page""
    select new { Key = doc.title, Value = doc.content, Size = doc.size };
";
            db.Indexes.PutIndex("pagesByTitle", new IndexDefinition{Map = definition});
            db.Indexes.DeleteIndex("pagesByTitle");
            var actualDefinition = db.IndexStorage.IndexNames.Where(x=>x.StartsWith("Raven") == false);
            Assert.Empty(actualDefinition);
        }
    }
}

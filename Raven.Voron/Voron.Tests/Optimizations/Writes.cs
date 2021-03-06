// -----------------------------------------------------------------------
//  <copyright file="Foo.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.IO;
using Voron.Debugging;
using Xunit;

namespace Voron.Tests.Optimizations
{
    public class Writes : StorageTest
    {
        [PrefixesFact]
        public void SinglePageModificationDoNotCauseCopyingAllIntermediatePages()
        {
            var keySize = 1024;
            using (var tx = Env.NewTransaction(TransactionFlags.ReadWrite))
            {
                tx.Root.Add(new string('9', keySize), new MemoryStream(new byte[3]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('1', keySize), new MemoryStream(new byte[3]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('4', 1000), new MemoryStream(new byte[2]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('5', keySize), new MemoryStream(new byte[2]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('8', keySize), new MemoryStream(new byte[3]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('2', keySize), new MemoryStream(new byte[2]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('6', keySize), new MemoryStream(new byte[2]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('0', keySize), new MemoryStream(new byte[4]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('3', 1000), new MemoryStream(new byte[1]));
                DebugStuff.RenderAndShow(tx.Root);
                tx.Root.Add(new string('7', keySize), new MemoryStream(new byte[1]));
                
                tx.Commit();
            }

            var afterAdds = Env.NextPageNumber;

            using (var tx = Env.NewTransaction(TransactionFlags.ReadWrite))
            {
                tx.Root.Delete(new string('0', keySize));

                tx.Root.Add(new string('4', 1000), new MemoryStream(new byte[21]));

                tx.Commit();
            }

            Assert.Equal(afterAdds, Env.NextPageNumber);

            // ensure changes were applied
            using (var tx = Env.NewTransaction(TransactionFlags.Read))
            {
                Assert.Null(tx.Root.Read(new string('0', keySize)));

                var readResult = tx.Root.Read(new string('4', 1000));

                Assert.Equal(21, readResult.Reader.Length);
            }
        }
    }
}

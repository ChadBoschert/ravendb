﻿// -----------------------------------------------------------------------
//  <copyright file="Aggregation.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Xunit;
using Raven.Client;

namespace Raven.Tests.Faceted
{
    public class Aggregation : RavenTest
    {
        public class Order
        {
            public string Product { get; set; }
            public decimal Total { get; set; }
            public Currency Currency { get; set; }
        }

        public enum Currency
        {
            USD,
            EUR,
            NIS
        }

        public class Orders_All : AbstractIndexCreationTask<Order>
        {
            public Orders_All()
            {
                Map = orders =>
                      from order in orders
                      select new { order.Currency, order.Product, order.Total };

                Sort(x => x.Total, SortOptions.Double);
            }
        }

        [Fact]
        public void CanCorrectlyAggregate()
        {
            using (var store = NewDocumentStore())
            {
                new Orders_All().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Order { Currency = Currency.EUR, Product = "Milk", Total = 3 });
                    session.Store(new Order { Currency = Currency.NIS, Product = "Milk", Total = 9 });
                    session.Store(new Order { Currency = Currency.EUR, Product = "iPhone", Total = 3333 });
                    session.SaveChanges();
                }
                WaitForIndexing(store);
                using (var session = store.OpenSession())
                {
                    var r = session.Query<Order, Orders_All>()
                           .AggregateBy(x => x.Product)
                           .SumOn(x => x.Total)
                           .ToList();

                    var facetResult = r.Results["Product"];
                    Assert.Equal(2, facetResult.Values.Count);

                    Assert.Equal(12, facetResult.Values.First(x => x.Range == "milk").Sum);
                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "iphone").Sum);

                }
            }
        }

        [Fact]
        public void CanCorrectlyAggregate_MultipleItems()
        {
            using (var store = NewDocumentStore())
            {
                new Orders_All().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Order { Currency = Currency.EUR, Product = "Milk", Total = 3 });
                    session.Store(new Order { Currency = Currency.NIS, Product = "Milk", Total = 9 });
                    session.Store(new Order { Currency = Currency.EUR, Product = "iPhone", Total = 3333 });
                    session.SaveChanges();
                }
                WaitForIndexing(store);
                using (var session = store.OpenSession())
                {
                    var r = session.Query<Order>("Orders/All")
                       .AggregateBy(order => order.Product)
                          .SumOn(order => order.Total)
                       .AndAggregateOn(order => order.Currency)
                           .SumOn(order => order.Total)
                       .ToList();

                    var facetResult = r.Results["Product"];
                    Assert.Equal(2, facetResult.Values.Count);

                    Assert.Equal(12, facetResult.Values.First(x => x.Range == "milk").Sum);
                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "iphone").Sum);

                    facetResult = r.Results["Currency"];
                    Assert.Equal(2, facetResult.Values.Count);

                    Assert.Equal(3336, facetResult.Values.First(x => x.Range == "eur").Sum);
                    Assert.Equal(9, facetResult.Values.First(x => x.Range == "nis").Sum);


                }
            }
        }

        [Fact]
        public void CanCorrectlyAggregate_MultipleAggregations()
        {
            using (var store = NewDocumentStore())
            {
                new Orders_All().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Order { Currency = Currency.EUR, Product = "Milk", Total = 3 });
                    session.Store(new Order { Currency = Currency.NIS, Product = "Milk", Total = 9 });
                    session.Store(new Order { Currency = Currency.EUR, Product = "iPhone", Total = 3333 });
                    session.SaveChanges();
                }
                WaitForIndexing(store);
                using (var session = store.OpenSession())
                {
                    var r = session.Query<Order>("Orders/All")
                       .AggregateBy(x => x.Product)
                         .MaxOn(x => x.Total)
                         .MinOn(x => x.Total)
                       .ToList();

                    var facetResult = r.Results["Product"];
                    Assert.Equal(2, facetResult.Values.Count);

                    Assert.Equal(9, facetResult.Values.First(x=>x.Range == "milk").Max);
                    Assert.Equal(3, facetResult.Values.First(x => x.Range == "milk").Min);

                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "iphone").Max);
                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "iphone").Min);

                }
            }
        }

        [Fact]
        public void CanCorrectlyAggregate_Ranges()
        {
            using (var store = NewDocumentStore())
            {
                new Orders_All().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Order { Currency = Currency.EUR, Product = "Milk", Total = 3 });
                    session.Store(new Order { Currency = Currency.NIS, Product = "Milk", Total = 9 });
                    session.Store(new Order { Currency = Currency.EUR, Product = "iPhone", Total = 3333 });
                    session.SaveChanges();
                }
                WaitForIndexing(store);
                using (var session = store.OpenSession())
                {
                    var r = session.Query<Order>("Orders/All")
                       .AggregateBy(x => x.Product)
                         .SumOn(x => x.Total)
                       .AndAggregateOn(x => x.Total)
                           .AddRanges(x => x.Total < 100,
                                      x => x.Total >= 100 && x.Total < 500,
                                      x => x.Total >= 500 && x.Total < 1500,
                                      x => x.Total >= 1500)
                       .SumOn(x => x.Total)
                       .ToList();

                    var facetResult = r.Results["Product"];
                    Assert.Equal(2, facetResult.Values.Count);

                    Assert.Equal(12, facetResult.Values.First(x => x.Range == "milk").Sum);
                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "iphone").Sum);

                    facetResult = r.Results["Total"];
                    Assert.Equal(4, facetResult.Values.Count);

                    Assert.Equal(12, facetResult.Values.First(x => x.Range == "[NULL TO Dx100]").Sum);
                    Assert.Equal(3333, facetResult.Values.First(x => x.Range == "{Dx1500 TO NULL]").Sum);


                }
            }
        }
    }
}
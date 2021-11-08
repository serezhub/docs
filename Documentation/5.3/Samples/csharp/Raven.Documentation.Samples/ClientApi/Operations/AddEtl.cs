﻿using System.Collections.Generic;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.ConnectionStrings;
using Raven.Client.Documents.Operations.ETL;
using Raven.Client.Documents.Operations.ETL.ElasticSearch;
using Raven.Client.Documents.Operations.ETL.OLAP;
using Raven.Client.Documents.Operations.ETL.SQL;
//using Raven.Client.Documents.Operations.ETL.OLAP;

namespace Raven.Documentation.Samples.ClientApi.Operations
{
    public class AddEtl
    {
        private interface IFoo
        {
            /*
            #region add_etl_operation
            public AddEtlOperation(EtlConfiguration<T> configuration)
            #endregion
            */
        }

        public AddEtl()
        {
            using (var store = new DocumentStore())
            {
                #region add_raven_etl

                AddEtlOperation<RavenConnectionString> operation = new AddEtlOperation<RavenConnectionString>(
                    new RavenEtlConfiguration
                    {
                        ConnectionStringName = "raven-connection-string-name",
                        Name = "Employees ETL",
                        Transforms =
                        {
                            new Transformation
                            {
                                Name = "Script #1",
                                Collections =
                                {
                                    "Employees"
                                },
                                Script = @"loadToEmployees ({
                                        Name: this.FirstName + ' ' + this.LastName,
                                        Title: this.Title
                                });"
                            }
                        }
                    });

                AddEtlOperationResult result = store.Maintenance.Send(operation);
                #endregion
            }


            using (var store = new DocumentStore())
            {
                #region add_sql_etl
                AddEtlOperation<SqlConnectionString> operation = new AddEtlOperation<SqlConnectionString>(
                    new SqlEtlConfiguration
                    {
                        ConnectionStringName = "sql-connection-string-name",
                        Name = "Orders to SQL",
                        SqlTables = {
                            new SqlEtlTable {TableName = "Orders", DocumentIdColumn = "Id", InsertOnlyMode = false},
                            new SqlEtlTable {TableName = "OrderLines", DocumentIdColumn = "OrderId", InsertOnlyMode = false},
                        },
                        Transforms =
                        {
                            new Transformation
                            {
                                Name = "Script #1",
                                Collections =
                                {
                                    "Orders"
                                },
                                Script = @"var orderData = {
                                                Id: id(this),
                                                OrderLinesCount: this.Lines.length,
                                                TotalCost: 0
                                            };

                                            for (var i = 0; i < this.Lines.length; i++) {
                                                var line = this.Lines[i];
                                                orderData.TotalCost += line.PricePerUnit;
                                                
                                                // Load to SQL table 'OrderLines'
                                                loadToOrderLines({
                                                    OrderId: id(this),
                                                    Qty: line.Quantity,
                                                    Product: line.Product,
                                                    Cost: line.PricePerUnit
                                                });
                                            }
                                            orderData.TotalCost = Math.round(orderData.TotalCost  * 100) / 100;

                                            // Load to SQL table 'Orders'
                                            loadToOrders(orderData)"
                            }
                        }
                    });

                AddEtlOperationResult result = store.Maintenance.Send(operation);
                #endregion
            }

            using (var store = new DocumentStore())
            {
                #region add_olap_etl
                AddEtlOperation<OlapConnectionString> operation = new AddEtlOperation<OlapConnectionString>(
                    new OlapEtlConfiguration
                    {
                        ConnectionStringName = "olap-connection-string-name",
                        Name = "Orders ETL",
                        Transforms =
                        {
                            new Transformation
                            {
                                Name = "Script #1",
                                Collections =
                                {
                                    "Orders"
                                },
                                Script = @"var orderDate = new Date(this.OrderedAt);
                                           var year = orderDate.getFullYear();
                                           var month = orderDate.getMonth();
                                           var key = new Date(year, month);

                                           loadToOrders(key, {
                                               Company : this.Company,
                                               ShipVia : this.ShipVia
                                           })"
                            }
                        }
                    });

                AddEtlOperationResult result = store.Maintenance.Send(operation);
                #endregion
            }

            using (var store = new DocumentStore())
            {
                #region create-connection-string
                // Create a Connection String to Elasticsearch
                var elasticSearchConnectionString = new ElasticSearchConnectionString
                {
                    // Connection String Name
                    Name = "ElasticConStr", 
                    // Elasticsearch Nodes URLs
                    Nodes = new[] { "http://localhost:9200" }, 
                    // Authentication Method
                    Authentication = new Raven.Client.Documents.Operations.ETL.ElasticSearch.Authentication 
                    { 
                        Basic = new BasicAuthentication
                        {
                            Username = "John",
                            Password = "32n4j5kp8"
                        }
                    }
                };

                store.Maintenance.Send(new PutConnectionStringOperation<ElasticSearchConnectionString>(elasticSearchConnectionString));
                #endregion

                #region add_elasticsearch_etl
                // Create an Elasticsearch ETL task
                AddEtlOperation<ElasticSearchConnectionString> operation = new AddEtlOperation<ElasticSearchConnectionString>(
                new ElasticSearchEtlConfiguration()
                {
                    ConnectionStringName = elasticSearchConnectionString.Name, // Connection String name
                    Name = "ElasticsearchEtlTask", // ETL Task name
                        
                    ElasticIndexes =
                    {
                        // Define Elasticsearch Indexes
                        new ElasticSearchIndex { // Elasticsearch Index name
                                                 IndexName = "orders", 
                                                 // Elasticsearch identifier for transferred RavenDB documents 
                                                 // (make sure a property with this name is defined in the transform script)
                                                 DocumentIdProperty = "DocId", 
                                                 // If true, don't send _delete_by_query before appending docs
                                                 InsertOnlyMode = false }, 
                        new ElasticSearchIndex { IndexName = "lines",
                                                 DocumentIdProperty = "OrderLinesCount", 
                                                 InsertOnlyMode = true 
                                               }
                    },
                    Transforms =
                    {   // Transformation script configuration
                        new Transformation()
                        {
                            Collections = { "Orders" }, // RavenDB collections that the script uses
                            Script = @"var orderData = {
                                       DocId: id(this),
                                       OrderLinesCount: this.Lines.length,
                                       TotalCost: 0
                                       };

                                       loadToOrders(orderData);", 
                            Name = "TransformIDsAndLinesCount" // Transformation script Name
                        }
                    }
                });

                store.Maintenance.Send(operation);
                #endregion
            }
        }
    }
}

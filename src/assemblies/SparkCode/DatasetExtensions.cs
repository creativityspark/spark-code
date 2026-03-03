using Microsoft.Xrm.Sdk;
using System.Data;

namespace SparkCode
{
    public static class DatasetExtensions
    {
        public static Entity ToEntity(this DataSet dataset)
        {
            Entity result = new Entity();

            // for each table in the dataset, add an attribute type EntityCollection.
            // The attribute name will be the same as the table name,
            // and the value will be an EntityCollection representing the rows in the table
            foreach (DataTable table in dataset.Tables)
            {
                EntityCollection collection = new EntityCollection();
                foreach (DataRow row in table.Rows)
                {
                    Entity entity = new Entity();
                    foreach (DataColumn column in table.Columns)
                    {
                        entity.Attributes.Add(column.ColumnName, row[column]);
                    }
                    collection.Entities.Add(entity);
                }
                result.Attributes.Add(table.TableName, collection);
            }
            return result;
        }
    }
}

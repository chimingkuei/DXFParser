using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DXFParser
{
    class WriteDXF
    {
        public void CreateLine(DxfDocument doc, Vector2 v1, Vector2 v2)
        {
            netDxf.Entities.Line entity = new netDxf.Entities.Line(v1, v2);
            doc.Entities.Add(entity);
        }

        public void CreateCircle(DxfDocument doc, Vector3 v, double d)
        {
            netDxf.Entities.Circle entity = new netDxf.Entities.Circle { Center = v, Radius = d };
            entity.Color = netDxf.AciColor.Blue;
            doc.Entities.Add(entity);
        }

        public void CreatePolyline2D(DxfDocument doc, Vector2[] points)
        {
            Polyline2D entity = new Polyline2D(points);
            entity.Color = netDxf.AciColor.Red;
            doc.Entities.Add(entity);
        }

    }

    class DataTransformer
    {
        private string[] GetListViewHeader(ListView listView)
        {
            List<string> headers = new List<string>();
            if (listView.View is GridView gridView)
            {
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        headers.Add(column.Header.ToString());
                    }
                }
            }
            return headers.ToArray();
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                // 如果字段包含逗号、双引号或换行符，用双引号包围字段，并将双引号转义为两个双引号
                field = "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private List<string[]> GetListViewContent(ListView listView)
        {
            List<string[]> data = new List<string[]>();
            foreach (var item in listView.Items)
            {
                if (item is CircleStruct circle)
                {
                    string[] content = new string[2];
                    content[0] = EscapeCsvField(circle.center.ToString());
                    content[1] = circle.radius.ToString();
                    data.Add(content);
                }
                if (item is LineStruct line)
                {
                    string[] content = new string[2];
                    content[0] = EscapeCsvField(line.startpoint.ToString());
                    content[1] = EscapeCsvField(line.endpoint.ToString());
                    data.Add(content);
                }
                if (item is Polylines2DStruct polylines2D)
                {
                    string[] content = new string[2];
                    content[0] = polylines2D.posX.ToString();
                    content[1] = polylines2D.posY.ToString();
                    data.Add(content);
                }
            }
            return data;
        }

        public void ExportCsv(ListView listView, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(string.Join(",", GetListViewHeader(listView)));
                foreach (var row in GetListViewContent(listView))
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }
            //範例:
            //string[] headers = { "Name", "Age", "City" };
            //string[][] data = {
            //new string[] { "Alice", "30", "New York" },
            //new string[] { "Bob", "25", "Los Angeles" },
            //new string[] { "Charlie", "35", "Chicago" }
            //};
        }

        
    }


}

using netDxf.Entities;
using netDxf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;

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

    class ReadDXF
    {
        public string JudgeStruct(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine("檔案不存在");
                return string.Empty; // 或 null 視需求
            }
            try
            {
                // 讀取第一行
                string firstLine = File.ReadLines(csvPath).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    Console.WriteLine("CSV 檔案是空的");
                    return string.Empty;
                }
                // 拆分欄位 (逗號或 tab 都可以)
                string[] columns = firstLine.Split(new[] { ',', '\t' }, StringSplitOptions.None);
                if (columns.Length > 0)
                {
                    string a1 = columns[0].Trim();
                    return a1; // 返回 A1 值
                }
                else
                {
                    Console.WriteLine("找不到欄位資料");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取 CSV 發生錯誤: {ex.Message}");
                return string.Empty;
            }
        }

        public void CreateCirclesFromCsv(WriteDXF WD, string csvPath, DxfDocument doc)
        {
            using (TextFieldParser parser = new TextFieldParser(csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");              // 逗號分隔
                parser.HasFieldsEnclosedInQuotes = true; // 支援雙引號欄位
                // 讀掉標題列
                if (!parser.EndOfData)
                    parser.ReadFields();
                int lineNumber = 1;
                while (!parser.EndOfData)
                {
                    lineNumber++;
                    string[] fields = parser.ReadFields();
                    if (fields.Length < 3)
                    {
                        Console.WriteLine($"第 {lineNumber} 行格式錯誤");
                        continue;
                    }
                    string centerStr = fields[1].Trim(); // "-772.818058258149, 246.715047361664, 0"
                    string radiusStr = fields[2].Trim();
                    // 拆分座標
                    string[] centerParts = centerStr.Split(',');
                    if (centerParts.Length < 3)
                    {
                        Console.WriteLine($"第 {lineNumber} 行中心座標格式錯誤: {centerStr}");
                        continue;
                    }
                    if (!double.TryParse(centerParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)) continue;
                    if (!double.TryParse(centerParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y)) continue;
                    if (!double.TryParse(centerParts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double z)) continue;
                    if (!double.TryParse(radiusStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double radius)) continue;
                    // 四捨五入到一位小數
                    x = Math.Round(x, 1);
                    y = Math.Round(y, 1);
                    z = Math.Round(z, 1);
                    // 呼叫 WD.CreateCircle
                    WD.CreateCircle(doc, new Vector3((float)x, (float)y, (float)z), radius);
                    Console.WriteLine($"第 {lineNumber} 行建立圓: ({x}, {y}, {z}), 半徑 {radius}");
                }
            }
        }

        public void CreateLinesFromCsv(WriteDXF WD, string csvPath, DxfDocument doc)
        {
            using (TextFieldParser parser = new TextFieldParser(csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");              // 逗號分隔
                parser.HasFieldsEnclosedInQuotes = true; // 支援雙引號欄位
                // 讀掉標題列
                if (!parser.EndOfData)
                    parser.ReadFields();
                int lineNumber = 1;
                while (!parser.EndOfData)
                {
                    lineNumber++;
                    string[] fields = parser.ReadFields();
                    if (fields.Length < 3)
                    {
                        Console.WriteLine($"第 {lineNumber} 行格式錯誤");
                        continue;
                    }
                    string startStr = fields[1].Trim().Trim('"'); // "-776.308058258144, 247.918012007228, 0"
                    string endStr = fields[2].Trim().Trim('"');   // "-774.021022903709, 250.205047361664, 0"
                    string[] startParts = startStr.Split(',');
                    string[] endParts = endStr.Split(',');
                    if (startParts.Length < 2 || endParts.Length < 2)
                    {
                        Console.WriteLine($"第 {lineNumber} 行座標格式錯誤");
                        continue;
                    }
                    if (!double.TryParse(startParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x1)) continue;
                    if (!double.TryParse(startParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y1)) continue;
                    if (!double.TryParse(endParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x2)) continue;
                    if (!double.TryParse(endParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y2)) continue;
                    // 四捨五入到一位小數
                    x1 = Math.Round(x1, 1);
                    y1 = Math.Round(y1, 1);
                    x2 = Math.Round(x2, 1);
                    y2 = Math.Round(y2, 1);
                    WD.CreateLine(doc, new Vector2((float)x1, (float)y1), new Vector2((float)x2, (float)y2));
                    Console.WriteLine($"第 {lineNumber} 行建立線段: Start({x1},{y1}) -> End({x2},{y2})");
                }
            }
        }

        public void CreatePolylineFromCsv(WriteDXF WD, string csvPath, DxfDocument doc, int pointsPerGroup)
        {
            if (pointsPerGroup < 3)
            {
                Console.WriteLine("每組至少需要 3 個點才能生成多邊形");
                return;
            }
            List<Vector2> points = new List<Vector2>();
            int lineCount = 0;
            using (TextFieldParser parser = new TextFieldParser(csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");               // 逗號分隔
                parser.HasFieldsEnclosedInQuotes = true; // 支援雙引號
                // 跳過標頭列
                if (!parser.EndOfData)
                    parser.ReadFields();
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields == null || fields.Length < 3) continue;
                    if (!double.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)) continue;
                    if (!double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double y)) continue;
                    points.Add(new Vector2((float)x, (float)y));
                    lineCount++;
                    // 每 pointsPerGroup 行生成一次多邊形
                    if (lineCount % pointsPerGroup == 0)
                    {
                        if (points.Count >= 3)
                        {
                            // 閉合多邊形
                            if (points[0] != points[points.Count - 1])
                                points.Add(points[0]);
                            WD.CreatePolyline2D(doc, points.ToArray());
                            Console.WriteLine($"已建立多邊形，共 {points.Count} 點");
                        }
                        points.Clear();
                    }
                }
                // 處理剩餘不足 pointsPerGroup 的最後一組
                if (points.Count >= 3)
                {
                    if (points[0] != points[points.Count - 1])
                        points.Add(points[0]);
                    WD.CreatePolyline2D(doc, points.ToArray());
                    Console.WriteLine($"已建立最後一個多邊形，共 {points.Count} 點");
                }
            }
        }


    }

    class DataHandler
    {
        public void CheckDir(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

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
                    string[] content = new string[3];
                    content[0] = circle.circle.ToString();
                    content[1] = EscapeCsvField(circle.center.ToString());
                    content[2] = circle.radius.ToString();
                    data.Add(content);
                }
                if (item is LineStruct line)
                {
                    string[] content = new string[3];
                    content[0] = line.line.ToString();
                    content[1] = EscapeCsvField(line.startpoint.ToString());
                    content[2] = EscapeCsvField(line.endpoint.ToString());
                    data.Add(content);
                }
                if (item is Polylines2DStruct polylines2D)
                {
                    string[] content = new string[3];
                    content[0] = polylines2D.polylines2D.ToString();
                    content[1] = polylines2D.posX.ToString();
                    content[2] = polylines2D.posY.ToString();
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
        }


    }
}

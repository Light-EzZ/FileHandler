using FileHandler.DAL;
using FileHandler.Models;
using HtmlAgilityPack; 
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing; 
using System.IO;
using System.Linq; 
using System.Text;
using System.Text.RegularExpressions; 
using System.Web; 

namespace FileHandler.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly FileContext _context;

        public DocumentService(FileContext context)
        {
            _context = context;
        }

       

        public byte[] ProcessFile(Stream fileStream, string fileName, string keywords, bool caseSensitive)//Початковий метод що перевіряє тип файлу та запускає відповідні методи
        {
            byte[] resultData;

            if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                resultData = HighlightPdf(fileStream, keywords, caseSensitive);
            }
            else if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                resultData = HighlightHtml(fileStream, keywords, caseSensitive);
            }
            else
            {
                throw new NotSupportedException("Тип файлу не підтримується.");
            }

           
            if (resultData != null && resultData.Length > 0)
            {
                var processedFileName = "processed_" + Path.GetFileName(fileName);
                var fileToSave = new StoredFile { Name = processedFileName, Data = resultData };
                _context.StoredFiles.Add(fileToSave);
                _context.SaveChanges();
            }

            return resultData;
        }


        //  Для PDF

        private byte[] HighlightPdf(Stream fileStream, string keywords, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                return ReadStreamToBytes(fileStream);

            // Розбиваємо ключові слова по пробілу 
            string[] allKeywords = keywords
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            using (PdfDocument doc = new PdfDocument())
            {
                doc.LoadFromStream(fileStream);

                foreach (PdfPageBase page in doc.Pages)
                {
                    //  Витягуємо текст сторінки
                    string pageText = page.ExtractText(); 

                    if (string.IsNullOrEmpty(pageText))
                        continue;

                    foreach (string keyword in allKeywords)
                    {
                        if (string.IsNullOrWhiteSpace(keyword))
                            continue;

                       
                        string pattern = Regex.Escape(keyword);

                       
                        var regexOptions = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                        // Усі входження
                        var matches = Regex.Matches(pageText, pattern, regexOptions);
                        if (matches.Count == 0)
                            continue;

                        foreach (Match m in matches)
                        {
                          
                            string foundText = m.Value;

                           
                            var textFinds = page.FindText(foundText, false);

                            if (textFinds?.Finds == null)
                                continue;

                            foreach (var find in textFinds.Finds)
                            {
                                find.ApplyHighLight(System.Drawing.Color.Yellow);
                            }
                        }
                    }
                }

                // Зберігаємо і повертаємо 
                using (MemoryStream ms = new MemoryStream())
                {
                    doc.SaveToStream(ms, FileFormat.PDF);
                    return ms.ToArray();
                }
            }
        }





        // Для HTML
        private byte[] HighlightHtml(Stream fileStream, string keywords, bool caseSensitive)
        {
            var doc = new HtmlDocument();
            doc.Load(fileStream, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(keywords))
            {
                // Повертаємо оригінал, якщо немає слів
               
                using (MemoryStream ms = new MemoryStream())
                {
                    doc.Save(ms, Encoding.UTF8);
                    return ms.ToArray();
                }
            }

            string[] allKeywords = keywords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var regexOptions = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            foreach (string keyword in allKeywords)
            {
               
                string regexPattern = $"({Regex.Escape(keyword)})";
                string replacement = "<mark>$1</mark>";

                //  Знаходимо текстові вузли
                var textNodes = doc.DocumentNode.DescendantsAndSelf()
                    .Where(n => n.NodeType == HtmlNodeType.Text &&
                                !n.ParentNode.Name.Equals("script") &&
                                !n.ParentNode.Name.Equals("style") &&
                                !n.ParentNode.Name.Equals("mark"))
                    .ToList(); //  викликToList() оскільки змінюється DOM

                foreach (var node in textNodes)
                {
                 
                    string decodedText = HttpUtility.HtmlDecode(node.InnerHtml);

                    string newHtml = Regex.Replace(
                        decodedText,
                        regexPattern,
                        replacement,
                        regexOptions
                    );

                    
                    if (newHtml != decodedText)
                    {
                        
                        var tempNode = HtmlNode.CreateNode("<div></div>");
                        tempNode.InnerHtml = newHtml;

                     
                        foreach (var child in tempNode.ChildNodes)
                        {
                            node.ParentNode.InsertBefore(child, node);
                        }

                       
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            //  Зберігаємо змінений HTML 
            using (MemoryStream ms = new MemoryStream())
            {
                doc.Save(ms, Encoding.UTF8);
                return ms.ToArray();
            }
        }

       
        private byte[] ReadStreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
using System.IO; 

namespace FileHandler.Services
{
   
    public interface IDocumentService
    {
        byte[] ProcessFile(Stream fileStream, string fileName, string keywords, bool caseSensitive);
    }
}
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using FileHandler.DAL;     
using FileHandler.Services;
namespace FileHandler
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            container.RegisterType<IDocumentService, DocumentService>();

            container.RegisterType<FileContext>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
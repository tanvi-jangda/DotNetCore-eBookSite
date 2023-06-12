using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Web.Areas.Customer.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace eBookSite.Web.Tests.System.Controller
{
    public class TestHomeController
    {
        private readonly HomeController _homeController;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        
        public TestHomeController()
        {
            mockUnitOfWork = new Mock<IUnitOfWork>();
            _homeController= new HomeController(mockUnitOfWork.Object);
        }

        //[Fact]
        //public void Index_ShouldReturnListOfProducts()
        //{
        //  mockUnitOfWork.Setup(m=>m.Product.GetAll(includeProperties:"Category")).Returns(new List<Product>());
        //}

        [Fact]
        public void Index_ShouldReturnIndexView()
        {
            // Arrange
            var mockObj= new Mock<IUnitOfWork>();
           var controller = new HomeController(mockObj.Object);
            

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
          
            Assert.IsType<ViewResult>(result);
        }
    }
}
    
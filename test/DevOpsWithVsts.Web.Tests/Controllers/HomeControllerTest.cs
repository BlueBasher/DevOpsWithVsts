namespace DevOpsWithVsts.Web.Tests.Controllers
{
    using DevOpsWithVsts.Web.Authentication;
    using DevOpsWithVsts.Web.Controllers;
    using DevOpsWithVsts.Web.FeatureFlag;
    using DevOpsWithVsts.Web.Todo;
    using FakeItEasy;
    using Socres.FakingEasy.AutoFakeItEasy;
    using System.Collections.Generic;
    using Xunit;

    public class HomeControllerTest
    {
        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_Index_RendersCorrectData(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            List<TodoItem> todos)
        {
            // Arrange
            A.CallTo(() => todoStorage.RetrieveAsync(A<string>.Ignored))
                .Returns(todos);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Index();

            // Assert
            Assert.IsType<System.Web.Mvc.ViewResult>(actual);
            Assert.Equal(todos, ((System.Web.Mvc.ViewResult)actual).Model);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_DetailsWithNullId_ReturnsBadRequest(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager)
        {
            // Arrange
            long? todoId = null;

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Details(todoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpStatusCodeResult>(actual);
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, ((System.Web.Mvc.HttpStatusCodeResult)actual).StatusCode);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_DetailsUnknownId_ReturnsHtppNotFound(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            long unknownTodoId)
        {
            // Arrange
            A.CallTo(() => todoStorage.RetrieveAsync(A<string>.Ignored, A<long>.Ignored))
                .Returns<TodoItem>(null);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Details(unknownTodoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpNotFoundResult>(actual);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_Details_RendersCorrectData(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            TodoItem todoItem)
        {
            // Arrange
            A.CallTo(() => claimsPrincipalService.UserId)
                .Returns(todoItem.UserId);
            A.CallTo(() => todoStorage.RetrieveAsync(todoItem.UserId, todoItem.Id))
                .Returns(todoItem);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Details(todoItem.Id);

            // Assert
            Assert.IsType<System.Web.Mvc.ViewResult>(actual);
            Assert.Equal(todoItem, ((System.Web.Mvc.ViewResult)actual).Model);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_EditWithNullId_ReturnsBadRequest(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager)
        {
            // Arrange
            long? todoId = null;

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Edit(todoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpStatusCodeResult>(actual);
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, ((System.Web.Mvc.HttpStatusCodeResult)actual).StatusCode);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_EditUnknownId_ReturnsHtppNotFound(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            long unknownTodoId)
        {
            // Arrange
            A.CallTo(() => todoStorage.RetrieveAsync(A<string>.Ignored, A<long>.Ignored))
                .Returns<TodoItem>(null);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Edit(unknownTodoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpNotFoundResult>(actual);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_Edit_RendersCorrectData(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            TodoItem todoItem)
        {
            // Arrange
            A.CallTo(() => claimsPrincipalService.UserId)
                .Returns(todoItem.UserId);
            A.CallTo(() => todoStorage.RetrieveAsync(todoItem.UserId, todoItem.Id))
                .Returns(todoItem);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Edit(todoItem.Id);

            // Assert
            Assert.IsType<System.Web.Mvc.ViewResult>(actual);
            Assert.Equal(todoItem, ((System.Web.Mvc.ViewResult)actual).Model);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_DeleteWithNullId_ReturnsBadRequest(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager)
        {
            // Arrange
            long? todoId = null;

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Delete(todoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpStatusCodeResult>(actual);
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, ((System.Web.Mvc.HttpStatusCodeResult)actual).StatusCode);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_DeleteUnknownId_ReturnsHtppNotFound(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            long unknownTodoId)
        {
            // Arrange
            A.CallTo(() => todoStorage.RetrieveAsync(A<string>.Ignored, A<long>.Ignored))
                .Returns<TodoItem>(null);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Delete(unknownTodoId);

            // Assert
            Assert.IsType<System.Web.Mvc.HttpNotFoundResult>(actual);
        }

        [Theory]
        [AutoFakeItEasyData]
        public async void HomeController_Delete_RendersCorrectData(
            ITodoStorage todoStorage,
            IClaimsPrincipalService claimsPrincipalService,
            IFeatureFlagManager featureFlagManager,
            TodoItem todoItem)
        {
            // Arrange
            A.CallTo(() => claimsPrincipalService.UserId)
                .Returns(todoItem.UserId);
            A.CallTo(() => todoStorage.RetrieveAsync(todoItem.UserId, todoItem.Id))
                .Returns(todoItem);

            // Act
            var homeController = new HomeController(todoStorage, claimsPrincipalService, featureFlagManager);
            var actual = await homeController.Delete(todoItem.Id);

            // Assert
            Assert.IsType<System.Web.Mvc.ViewResult>(actual);
            Assert.Equal(todoItem, ((System.Web.Mvc.ViewResult)actual).Model);
        }
    }
}

using Xunit;
using Moq;
using ApiTask.Controllers;
using ApiTask.Repositories;
using ApiTask.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTask.DTOs;

public class TasksControllerTests
{
    [Fact]
    public async Task GetTasks_ReturnsOkResult_WithAListOfTasks()
    {
        // Arrange:
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(repo => repo.GetTasks())
                .ReturnsAsync(GetTestTasks());

        var controller = new TaskApiController(mockRepo.Object);

        // Act:
        var result = await controller.GetTasks();

        // Assert:
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<MyTask>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }
    private List<MyTask> GetTestTasks()
    {
        return new List<MyTask>
        {
            new MyTask { Id = Guid.NewGuid(), Title = "Task 1", Description = "Description 1" },
            new MyTask { Id = Guid.NewGuid(), Title = "Task 2", Description = "Description 2" },
        };
    }

    [Fact]
    public async Task GetTaskById_ReturnsOkResult_WithTask()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        mockRepo.Setup(repo => repo.GetTaskById(taskId))
                .ReturnsAsync(new MyTask { Id = taskId, Title = "Task 1", Description = "Description 1" });

        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.GetTask(taskId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<MyTask>(okResult.Value);
        Assert.Equal(taskId, returnValue.Id);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskNotExists()
    {
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        mockRepo.Setup(repo => repo.GetTaskById(taskId))
                .ReturnsAsync((MyTask)null);

        var controller = new TaskApiController(mockRepo.Object);

        var result = await controller.GetTask(taskId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var newTask = new CreateTaskDto { Title = "New Task", Description = "New Description" };
        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.CreateTask(newTask);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetTask", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var controller = new TaskApiController(mockRepo.Object);

        // Agrega manualmente un error al ModelState para simular la validación fallida
        controller.ModelState.AddModelError("Title", "Title cannot be longer than 50 characters.");

        var createTaskDto = new CreateTaskDto
        {
            Title = new string('A', 51), // 51 caracteres
            Description = "Valid Description"
        };

        // Act
        var result = await controller.CreateTask(createTaskDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(modelState.ContainsKey("Title"));
    }

    [Fact]
    public async Task UpdateTask_ReturnsNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Description" };

        mockRepo.Setup(repo => repo.GetTaskById(taskId))
                .ReturnsAsync(new MyTask { Id = taskId, Title = "Task 1", Description = "Description 1" });

        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.UpdateTask(taskId, updateTaskDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockRepo.Verify(repo => repo.UpdateTask(It.IsAny<MyTask>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Description" };

        mockRepo.Setup(repo => repo.GetTaskById(taskId))
                .ReturnsAsync((MyTask)null);

        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.UpdateTask(taskId, updateTaskDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContentResult_WhenTaskIsDeleted()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        mockRepo.Setup(repo => repo.DeleteTask(taskId))
                .ReturnsAsync(true);

        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.DeleteTask(taskId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFoundResult_WhenTaskDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var taskId = Guid.NewGuid();
        mockRepo.Setup(repo => repo.DeleteTask(taskId))
                .ReturnsAsync(false);

        var controller = new TaskApiController(mockRepo.Object);

        // Act
        var result = await controller.DeleteTask(taskId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

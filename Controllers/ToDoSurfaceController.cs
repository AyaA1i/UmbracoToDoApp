using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;
using System;

namespace YourProjectNamespace.Controllers
{
    public class ToDoSurfaceController : SurfaceController
    {
        private readonly IContentService _contentService;
        private readonly ILogger<ToDoSurfaceController> _logger;

        public ToDoSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IContentService contentService,
            ILogger<ToDoSurfaceController> logger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentService = contentService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string taskTitle, string description, DateTime? dueDate, bool isCompleted = false, int parentId = 0)
        {
            try
            {

                if (parentId == 0)
                {
                    TempData["Error"] = "Parent ID is required";
                    return RedirectToCurrentUmbracoPage();
                }

                if (string.IsNullOrWhiteSpace(taskTitle))
                {
                    TempData["Error"] = "Task title is required";
                    return RedirectToCurrentUmbracoPage();
                }

                // Create new content item
                var newTask = _contentService.Create(taskTitle, parentId, "toDoItem");

                // Set properties
                newTask.SetValue("taskTitle", taskTitle);
                newTask.SetValue("description", description ?? string.Empty);
                newTask.SetValue("dueDate", dueDate);
                newTask.SetValue("isCompleted", isCompleted);

                // First save the content
                var saveResult = _contentService.Save(newTask);

                if (saveResult.Success)
                {
                    // For invariant content, use empty array or wildcard for cultures
                    var publishResult = _contentService.Publish(newTask, new string[] { }); // Empty array for invariant content

                    if (publishResult.Success)
                    {
                        TempData["Success"] = "Task created successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                TempData["Error"] = "Error creating task: " + ex.Message;
            }

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int taskId, string taskTitle, string description, DateTime? dueDate, bool isCompleted = false)
        {
            try
            {
                _logger.LogInformation("=== UPDATE METHOD CALLED ===");
                _logger.LogInformation($"taskId: {taskId}");
                _logger.LogInformation($"taskTitle: {taskTitle}");
                _logger.LogInformation($"description: {description}");
                _logger.LogInformation($"dueDate: {dueDate}");
                _logger.LogInformation($"isCompleted: {isCompleted}");

                var task = _contentService.GetById(taskId);
                if (task != null)
                {
                    if (string.IsNullOrWhiteSpace(taskTitle))
                    {
                        TempData["Error"] = "Task title is required";
                        return RedirectToCurrentUmbracoPage();
                    }

                    // Update properties
                    task.SetValue("taskTitle", taskTitle);
                    task.SetValue("description", description ?? string.Empty);
                    task.SetValue("dueDate", dueDate);
                    task.SetValue("isCompleted", isCompleted);
                    task.Name = taskTitle; // Update the node name as well

                    // Save and publish
                    var saveResult = _contentService.Save(task);
                    
                    if (saveResult.Success)
                    {
                        // Use empty array for invariant content
                        var publishResult = _contentService.Publish(task, new string[] { });

                        if (publishResult.Success)
                        {
                            TempData["Success"] = "Task updated successfully!";
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Task not found!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                TempData["Error"] = "Error updating task: " + ex.Message;
            }

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int taskId)
        {
            try
            {
                _logger.LogInformation("=== DELETE METHOD CALLED ===");
                _logger.LogInformation($"taskId: {taskId}");

                var task = _contentService.GetById(taskId);
                if (task != null)
                {
                    var deleteResult = _contentService.Delete(task);
                    if (deleteResult.Success)
                    {
                        TempData["Success"] = "Task deleted successfully!";
                    }
                }
                else
                {
                    TempData["Error"] = "Task not found!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task");
                TempData["Error"] = "Error deleting task: " + ex.Message;
            }

            return RedirectToCurrentUmbracoPage();
        }
    }
}
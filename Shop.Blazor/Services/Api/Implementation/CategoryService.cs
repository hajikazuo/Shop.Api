﻿using Shop.Blazor.Services.Api.Interface;
using Shop.Common.Models.DTO.Category;
using Shop.Common.Models.Entities;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Net;
using System;

namespace Shop.Blazor.Services.Api.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ILogger<CategoryService> _logger;
        private const string apiEndpoint = "/api/categories/";
        private readonly JsonSerializerOptions _options;

        private CategoryResponseDto? category;
        private IEnumerable<CategoryResponseDto>? categories;

        public CategoryService(IHttpClientFactory httpClientFactory,
            ILogger<CategoryService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<CategoryResponseDto>> GetAll()
        {
            var httpClient = _httpClientFactory.CreateClient("Shop.Api");
            try
            {
                return await httpClient.GetFromJsonAsync<List<CategoryResponseDto>>(apiEndpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao acessar: {apiEndpoint} " + ex.Message);
                throw new UnauthorizedAccessException();
            }
        }

        public async Task<CategoryResponseDto> GetById(Guid id)
        {
            var httpClient = _httpClientFactory.CreateClient("Shop.Api");
            var response = await httpClient.GetAsync(apiEndpoint + id);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao obter pelo id= {id} - {message}");
                throw new Exception($"Status Code : {response.StatusCode} - {message}");
            }

            return await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
        }

        public async Task<CategoryResponseDto> Create(CategoryRequestDto categoryDto)
        {
            var httpClient = _httpClientFactory.CreateClient("Shop.Api");
            var content = new StringContent(JsonSerializer.Serialize(categoryDto), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                return null;
            }

            var apiResponse = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<CategoryResponseDto>(apiResponse, _options);
        }

        public async Task<CategoryResponseDto> Update(Guid id, CategoryRequestDto categoryDto)
        {
            var httpClient = _httpClientFactory.CreateClient("Shop.Api");

            var response = await httpClient.PutAsJsonAsync(apiEndpoint + id, categoryDto);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                return null;
            }

            var apiResponse = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<CategoryResponseDto>(apiResponse, _options);
        }


        public async Task<bool> Delete(Guid id)
        {
            var httpClient = _httpClientFactory.CreateClient("Shop.Api");
            var response = await httpClient.DeleteAsync(apiEndpoint + id);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                return false;
            }

            return true;
        }
    }
}

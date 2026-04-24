using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetAll()
        {
            var products = await _context.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToListAsync();
            return products;
        }

        public async Task<ProductDto> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
             
        }
        public async Task Create(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task Update(int id, CreateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

        }
        public async Task Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return; 
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

        }

        
    }
}

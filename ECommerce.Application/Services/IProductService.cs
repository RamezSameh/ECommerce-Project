using ECommerce.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAll();
        Task<ProductDto> GetById(int id);
        Task Create(CreateProductDto dto);
        Task Update(int id, CreateProductDto dto);
        Task Delete(int id);
    }
}

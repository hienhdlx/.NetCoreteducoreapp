using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Implementation
{
    public class BillService: IBillService
    {
        private readonly IBillRepository _orderRepository;
        private readonly IBillDetailRepository _orderDetailRepository;
        private readonly IColorRepository _colorRepository;
        private readonly ISizeRepository _sizeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BillService(IBillRepository orderRepository,
            IBillDetailRepository orderDetailRepository,
            IColorRepository colorRepository,
            IProductRepository productRepository,
            ISizeRepository sizeRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _colorRepository = colorRepository;
            _sizeRepository = sizeRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public void Create(BillViewModel billVm)
        {
            var order = Mapper.Map<BillViewModel, Bill>(billVm);
            var orderDetails = Mapper.Map<List<BillDetailViewModel>, List<BillDetail>>(billVm.BillDetails);
            foreach (var detail in orderDetails)
            {
                var product = _productRepository.FindById(detail.ProductId);
                detail.Price = product.Price;
            }

            order.BillDetails = orderDetails;
            _orderRepository.Add(order);
        }

        public void Update(BillViewModel billVm)
        {
            //mapping into order domain
            var order = Mapper.Map<BillViewModel, Bill>(billVm);

            //get order detail
            var newDetails = order.BillDetails;

            //new detail added
            var addedDetails = newDetails.Where(x => x.Id == 0).ToList();

            //get updateed details
            var updateDetails = newDetails.Where(x => x.Id != 0).ToList();

            //Existed details
            var existdetails = _orderDetailRepository.FindAll(x => x.BillId == billVm.Id);

            //Clear db
            order.BillDetails.Clear();

            foreach (var detail in updateDetails)
            {
                var product = _productRepository.FindById(detail.ProductId);
                detail.Price = product.Price;
                _orderDetailRepository.Update(detail);
            }

            foreach (var detail in addedDetails)
            {
                var product = _productRepository.FindById(detail.ProductId);
                detail.Price = product.Price;
                _orderDetailRepository.Add(detail);
            }

            _orderDetailRepository.RemoveMultiple(existdetails.Except(updateDetails).ToList());
            _orderRepository.Update(order);
        }

        public PageResult<BillViewModel> GetAllPaging(string startDate, string endDate, string keyword, int pageIndex, int pageSize)
        {
            var query = _orderRepository.FindAll();
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime start = DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.DateCreated >= start);
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime end = DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.DateModified <= end);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.CustomerName.Contains(keyword) || x.CustomerMobile.Contains(keyword));
            }

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.DateCreated)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<BillViewModel>()
                .ToList();
            return new PageResult<BillViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount =  totalRow,
                Results = data
            };

        }

        public BillViewModel GetDetail(int billId)
        {
            var bill = _orderRepository.FindSingle(x =>  x.Id == billId);
            var billVm = Mapper.Map<Bill, BillViewModel>(bill);
            var billDetailVm = _orderDetailRepository.FindAll(x => x.BillId == billId).ProjectTo<BillDetailViewModel>()
                .ToList();
            billVm.BillDetails = billDetailVm;
            return billVm;
        }

        public BillDetailViewModel CreateDetail(BillDetailViewModel billDetailVm)
        {
            var billDetail = Mapper.Map<BillDetailViewModel, BillDetail>(billDetailVm);
            _orderDetailRepository.Add(billDetail);
            return billDetailVm;
        }

        public void DeleteDetail(int productId, int billId, int colorId, int sizeId)
        {
            var detail = _orderDetailRepository.FindSingle(x => x.ProductId == productId && x.BillId == billId && x.ColorId == colorId && x.SizeId == sizeId);
            _orderDetailRepository.Remove(detail);
        }

        public void UpdateStatus(int orderId, BillStatus status)
        {
            var order = _orderRepository.FindById(orderId);
            order.BillStatus = status;
            _orderRepository.Update(order);
        }

        public List<BillDetailViewModel> GetBillDetails(int billId)
        {
            return _orderDetailRepository
                .FindAll(x => x.BillId == billId, c => c.Bill, c => c.Color, c => c.Size, c => c.Product)
                .ProjectTo<BillDetailViewModel>().ToList();
        }

        public List<ColorViewModel> GetColors()
        {
            return _colorRepository.FindAll().ProjectTo<ColorViewModel>().ToList();
        }

        public List<SizeViewModel> GetSizes()
        {
            return _sizeRepository.FindAll().ProjectTo<SizeViewModel>().ToList();
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public ColorViewModel GetColor(int id)
        {
            return Mapper.Map<Color, ColorViewModel>(_colorRepository.FindById(id));
        }

        public SizeViewModel GetSize(int id)
        {
            return Mapper.Map<Size, SizeViewModel>(_sizeRepository.FindById(id));
        }
    }
}

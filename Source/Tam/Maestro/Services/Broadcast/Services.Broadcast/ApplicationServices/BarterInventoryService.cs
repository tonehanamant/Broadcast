using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices
{
    public interface IBarterInventoryService : IApplicationService
    {
        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName);
    }

    public class BarterInventoryService : IBarterInventoryService
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IBarterRepository _BarterRepository;
        private readonly IBarterFileImporter _BarterFileImporter;

        public BarterInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IBarterFileImporter barterFileImporter)
        {
            _BarterRepository = broadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _BarterFileImporter = barterFileImporter;
        }

        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        public InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName)
        {
            _BarterFileImporter.LoadFromSaveRequest(request);
            BarterInventoryFile barterFile = _BarterFileImporter.GetPendingBarterInventoryFile(userName);
            _BarterFileImporter.CheckFileHash();

            barterFile.Id = _InventoryFileRepository.CreateInventoryFile(barterFile, userName);
            try
            {
                _BarterFileImporter.ExtractData(barterFile);
                barterFile.FileStatus = barterFile.ValidationProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                using (var transaction = new TransactionScopeWrapper()) //Ensure database request succeeds or fails
                {
                    if (barterFile.ValidationProblems.Any())
                    {
                        _BarterRepository.AddValidationProblems(barterFile);
                    }
                    else
                    {
                        _BarterRepository.SaveBarterInventoryFile(barterFile);
                    }
                    
                    transaction.Complete();
                }
            }
            catch(Exception ex)
            {
                barterFile.ValidationProblems.Add(ex.Message);
                barterFile.FileStatus = FileStatusEnum.Failed;
                _BarterRepository.AddValidationProblems(barterFile);
            }
            finally
            {
                //unlock stations however you want
            }
            

            return new InventoryFileSaveResult
            {
                FileId = barterFile.Id,
                Status = barterFile.FileStatus,
                ValidationProblems = barterFile.ValidationProblems
            };
        }
    }
}

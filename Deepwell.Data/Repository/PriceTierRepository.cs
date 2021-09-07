using Deepwell.Common.CommonModel.PriceTier;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public class PriceTierRepository : IPriceTier
    {
        private DeepwellContext _deepwellContext;

        public PriceTierRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public PriceTierRepository(DeepwellContext deepwellContext)
        {
            _deepwellContext = deepwellContext;
        }

        public Tier AddTier(Tier tier)
        {
            return _deepwellContext
                .Tiers
                .Add(tier);
        }

        public bool Add(PriceTierModel request)
        {
            try
            {
                Tier newTier = AddTier(new Tier
                {
                    Title = request.Title,
                    IsActive = request.IsActive,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                });

                this.AddProducts(newTier.TierId, request.TierProducts);

                return _deepwellContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public IEnumerable<Tier> GetTiers()
        {
            return _deepwellContext
                .Tiers
                .OrderBy(l => l.Title);
        }

        public Tier GetById(int Id)
        {
            return _deepwellContext.Tiers
                 .FirstOrDefault(t => t.TierId == Id);
        }

        public IEnumerable<Tier> GetAllActive()
        {
            return _deepwellContext
                .Tiers
                .Where(t => t.IsActive == true);
        }

        public TierDetail GetTierProductById(int productId, int tierId)
        {
            return _deepwellContext.TierDetails
                .FirstOrDefault(t => t.ProductId == productId && t.TierId == tierId);
        }

        public bool Edit(PriceTierModel model)
        {
            var tier = GetById(model.TierId);
            if (tier.IsNotNull())
            {
                tier.Title = model.Title;
                tier.IsActive = model.IsActive;

                _deepwellContext.Entry(tier).State = System.Data.Entity.EntityState.Modified;

                this.AddProducts(model.TierId, model.TierProducts);
                return _deepwellContext.SaveChanges() > 0;
            }

            return false;
        }

        public void AddProducts(int tierId, IEnumerable<TierProduct> products)
        {
            if (products.Any())
            {
                products.ToList().ForEach(p =>
                {
                    switch (p.Status)
                    {
                        case PriceTierProductStatus.New:
                            {
                                var tierDetail = new TierDetail
                                {
                                    TierId = tierId,
                                    ProductId = p.ProductId,
                                    ProductNumber = p.ProductNumber,
                                    Price = p.Price,
                                    DateCreated = DateTime.Now,
                                    DateModified = DateTime.Now,
                                };

                                _deepwellContext.TierDetails.Add(tierDetail);
                                break;
                            }

                        case PriceTierProductStatus.Removed:
                            {
                                var tierProduct = this.GetTierProductById(p.ProductId, tierId);
                                _deepwellContext.TierDetails.Remove(tierProduct);
                                break;
                            }

                        case PriceTierProductStatus.Updated:
                            {
                                var tierProduct = this.GetTierProductById(p.ProductId, tierId);
                                tierProduct.Price = p.Price;
                                tierProduct.DateModified = DateTime.Now;
                                _deepwellContext.Entry(tierProduct).State = System.Data.Entity.EntityState.Modified;
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }
                });
            }
        }
    }
}

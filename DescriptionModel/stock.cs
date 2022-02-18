using System;
using System.Collections.Generic;
using System.Text;

namespace DescriptionModel.stock {
    public class Stock {
        public int Id { get; set; }
        /// <summary>
        /// day week,minute,5minute,15minute
        /// </summary>
        public TimeSpan TimeRange { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
    }
    /// <summary>
    /// 上市公司财报
    /// </summary>
    public class StoreCompanyFinancial {
        ///<summary>基本每股收益</summary>
        public string BasicEarningsPerShare { get; set; }
        ///<summary>扣非每股收益</summary>
        public string BuckleTheEarningsPerShare { get; set; }
        ///<summary>稀释每股收益</summary>
        public string DilutedEarningsPerShare { get; set; }
        ///<summary>每股净资产</summary>
        public string NetAssetsPerShare { get; set; }
        ///<summary>每股资本公积</summary>
        public string CapitalReservesPerShare { get; set; }
        ///<summary>每股未分配利润</summary>
        public string UndistributedProfitPerShare { get; set; }
        ///<summary>每股经营现金流</summary>
        public string OperatingCashFlowPerShare { get; set; }
        ///<summary>营业总收入</summary>
        public string OperatingIncome { get; set; }
        ///<summary>毛利润</summary>
        public string GrossMargin { get; set; }
        ///<summary>归属净利润</summary>
        public string OwnershipOfNetIncome { get; set; }
        ///<summary>扣非净利润</summary>
        public string BuckleTheNetIncome { get; set; }
        ///<summary>营业总收入同比增长</summary>
        public string OperatingIncomeRose { get; set; }
        ///<summary>归属净利润同比增长</summary>
        public string BelongingToNetProfitYearOnYearGrowth { get; set; }
        ///<summary>扣非净利润同比增长</summary>
        public string TheNetProfitYearOnYearGrowth { get; set; }
        ///<summary>营业总收入滚动环比增长</summary>
        public string OperatingIncomeRoseScrolling { get; set; }
        ///<summary>归属净利润滚动环比增长</summary>
        public string BelongingToNetProfitRoseScrolling { get; set; }
        ///<summary>扣非净利润滚动环比增长</summary>
        public string BuckleTheNetProfitRoseScrolling { get; set; }
        ///<summary>净资产收益率</summary>
        public string ReturnOnEquity { get; set; }
        ///<summary>净资产收益率(扣非/加权)</summary>
        public string ReturnOnEquityBuckleWeighted { get; set; }
        ///<summary>总资产收益率</summary>
        public string TheTotalReturnOnAssets { get; set; }
        ///<summary>毛利率</summary>
        public string GrossProfitMargin { get; set; }
        ///<summary>净利率</summary>
        public string TheNetInterestRate { get; set; }
        ///<summary>预收账款/营业收入</summary>
        public string DeferredRevenueRevenue { get; set; }
        ///<summary>销售净现金流/营业收入</summary>
        public string NetCashFlowsOfSalesRevenue { get; set; }
        ///<summary>经营净现金流/营业收入</summary>
        public string BusinessNetCashFlowRevenue { get; set; }
        ///<summary>实际税率</summary>
        public string EffectiveTaxRate { get; set; }
        ///<summary>流动比率</summary>
        public string CurrentRatio { get; set; }
        ///<summary>速动比率</summary>
        public string QuickRatio { get; set; }
        ///<summary>现金流量比率</summary>
        public string CashFlowRatio { get; set; }
        ///<summary>资产负债率</summary>
        public string AssetLiabilityRatio { get; set; }
        ///<summary>权益乘数</summary>
        public string TheRightsAndInterestsMultiplier { get; set; }
        ///<summary>产权比率</summary>
        public string EquityRatio { get; set; }
        ///<summary>总资产周转天数</summary>
        public string TotalAssetsTurnoverDays { get; set; }
        ///<summary>应收账款周转天数</summary>
        public string AccountsReceivableTurnoverDays { get; set; }
        ///<summary>总资产周转率</summary>
        public string TotalAssetTurnover { get; set; }
        ///<summary>应收账款周转率</summary>
        public string AccountsReceivableTurnoverRatio { get; set; }
    }
    public class StoreCompanyInfo {
        ///<summary>公司名称</summary>
        public string CompanyName { get; set; }
        ///<summary>曾用名</summary>
        public string FormerName { get; set; }
        ///<summary>A股代码</summary>
        public string ASharesCode { get; set; }
        ///<summary>A股简称</summary>
        public string ASharesReferredToAs { get; set; }
        ///<summary>所属区域</summary>
        public string Area { get; set; }
        ///<summary>所属行业</summary>
        public string SubordinateToIndustry { get; set; }
        ///<summary>所属概念</summary>
        public string SubordinateToConcept { get; set; }
        ///<summary>董事长</summary>
        public string ChairmanOfBoardOfDirectors { get; set; }
        ///<summary>法人代表</summary>
        public string LegalRepresentative { get; set; }
        ///<summary>总经理</summary>
        public string GeneralManager { get; set; }
        ///<summary>董秘</summary>
        public string ChairmanSecretary { get; set; }
        ///<summary>成立日期</summary>
        public string SetUpDate { get; set; }
        ///<summary>注册资本</summary>
        public string RegisteredCapital { get; set; }
        ///<summary>员工人数</summary>
        public string NumberOfEmployees { get; set; }
        ///<summary>管理层人数</summary>
        public string NumberOfManagement { get; set; }
        ///<summary>联系电话</summary>
        public string ContactPhoneNumber { get; set; }
        ///<summary>电子邮箱</summary>
        public string Email { get; set; }
        ///<summary>公司网址</summary>
        public string CompanyWebSite { get; set; }
        ///<summary>办公地址</summary>
        public string OfficeAddress { get; set; }
        ///<summary>注册地址</summary>
        public string RegisteredAddress { get; set; }
        ///<summary>公司简介</summary>
        public string CompanyProfile { get; set; }
        ///<summary>主营业务</summary>
        public string MainBusiness { get; set; }
    }
}

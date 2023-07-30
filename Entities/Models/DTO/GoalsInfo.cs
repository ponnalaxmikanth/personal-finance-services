using System;
using System.Collections.Generic;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;
using Excel.FinancialFunctions;
using System.Linq;

namespace Entities.Models.DTO
{
    public class PortfolioGoals
    {
        public Portfolio Portfolio { get; set; }
        public ConsolidatedGoalInfo Consolidated { get; set; }
        //public List<GoalsInfo> Goals { get; set; }
    }

    public class ConsolidatedGoalInfo
    {
        public decimal MonthlyInvestment { get; set; }
        public decimal TargetValue { get; set; }
        public decimal TargetWithInflation { get; set; }
        public decimal CurrentInvestment { get; set; }
        public decimal TargetAchieved { get; set; }
    }

    //public class GoalsInfo
    //{
    //    public decimal Percent { get; set; }

    //    public decimal FolioValue { get; set; }
    //    public decimal CurrentValue { get; set; }

    //    public int CompletedMonths { get; set; }
    //    public int RemainingMonths { get; set; }

    //    public decimal RemainingPercent { get; set; }

    //    public decimal Investment { get; set; }
    //    public decimal ExpectedInvestment { get; set; }
    //    public decimal ExpectedProfit { get; set; }

    //    public decimal ExpectedInvestPer { get; set; }
    //    public decimal ExpectedProfitPer { get; set; }

    //    public Goal Goal { get; set; }
    //}

    public class PortfolioData
    {
        public Consolidated Consolidated { get; set; }
        public ConsolidatedGoal ConsolidatedInfo { get; set; }
        public List<Goal> Goals { get; set; }
        public List<Goal> CurrentMonth { get; set; }
        public List<CategoryValuation> CategoryValuation { get; set; }
        public List<FinancialYearValuation> FinancialYearValuation { get; set; }

        public List<FundInvestment> Transactions { get; set; }
        //public List<FundsDailyTrack> FundsDailyTrack { get; set; }

        public List<MonthView> MonthlyTracker { get; set; }
        public List<MonthlyGoalView> MonthlyGoalView { get; set; }

        public YearOldInvestReview YearOldInvestReview { get; set; }
    }

    public class YearOldInvestReview
    {
        public double Investment { get; set; }
        public double CurrentValue { get; set; }
        public double Profit => CurrentValue - Investment;
        public double ProfitPer => Profit / Investment * 100;
        public decimal XIRR { get; set; }
    }

    public class GoalsInfo
    {
        public ConsolidatedGoal ConsolidatedInfo { get; set; }
        public List<Goal> Goals { get; set; }
    }

    public class ConsolidatedGoal
    {
        public decimal SIPAmount { get; set; }

        public decimal PresentValue { get; set; }
        public decimal FutureValue { get; set; }
        public decimal Target { get; set; }

        public decimal CurrentInvestment { get; set; }
        public decimal CurrentValue { get; set; }
        
        public decimal CurrentMonthInvestment { get; set; }
        public decimal CurrentMonthValue { get; set; }
        public decimal ExpectedInvestment { get; set; }

        public decimal CurrentMonthSIPPercent => SIPAmount == 0 ? 0 : CurrentMonthInvestment / SIPAmount * 100;
        public decimal CurrentProfit => CurrentValue - CurrentInvestment;
        public decimal CurrentProfitPer => CurrentInvestment == 0 ? 0 : CurrentProfit / CurrentInvestment * 100;
        public decimal CurrentMonthProfit => CurrentMonthValue - CurrentMonthInvestment;
        public decimal CurrentMonthProfitPer => CurrentMonthInvestment == 0 ? 0 : CurrentMonthProfit / CurrentMonthInvestment * 100;

        public decimal TargetAchieved => Target == 0 ? 0 : CurrentValue / Target * 100;
        public decimal TargetInvested => ExpectedInvestment == 0 ? 0 : CurrentInvestment / ExpectedInvestment * 100;

        public DateTime StartDate { get; set; }
        public int SIPDuration { get; set; }
        public int TargetDuration { get; set; }

        //protected DateTime SIPEndDate => StartDate == DateTime.MinValue ? StartDate : StartDate.AddYears(SIPDuration).AddDays(-1);
        public DateTime EndDate { get; set; } // => StartDate == DateTime.MinValue ? StartDate : StartDate.AddYears(TargetDuration).AddDays(-1);

        public int CompletedMonths => ((DateTime.Now.Year - StartDate.Year) * 12) + DateTime.Now.Month - StartDate.Month;
        public int RemainingMonths => ((EndDate.Year - DateTime.Now.Year) * 12) + EndDate.Month - DateTime.Now.Month + 1;
        public int TotalMonths => CompletedMonths + RemainingMonths;
        public decimal TargetInvestment { get; set; }
    }

    public class Goal
    {
        public int GoalID { get; set; }
        public string GoalName { get; set; }

        public int PortfolioID { get; set; }
        public string Portfolio { get; set; }

        public DateTime StartDate { get; set; }
        public double Inflation { get; set; }
        public double PresentValue { get; set; }

        public decimal Investment { get; set; }
        public decimal CurrentValue { get; set; }

        public int SIPDuration { get; set; }
        public int TargetDuration { get; set; }
        public double ExpectedCAGR => 15;

        public DateTime SIPEndDate => StartDate.AddYears(SIPDuration).AddDays(-1);
        public DateTime EndDate => StartDate.AddYears(TargetDuration).AddDays(-1);

        public decimal SIPAmount { get; set; }
        //public decimal GoalInvestValue { get; set; }

        public decimal ExpectedInvestment => SIPDuration * Convert.ToDecimal(SIPAmount) * 12;

        public double FutureValue => Financial.Fv(Inflation / 100, TargetDuration, 0, -PresentValue, PaymentDue.EndOfPeriod);

        //private DateTime now = DateTime.Now;
        private DateTime nextMonth = DateTime.Now.AddMonths(1);

        public double SIPEndFutureValue => SIPDuration == TargetDuration ? FutureValue : Financial.Pv(ExpectedCAGR / 100, (TargetDuration - SIPDuration), 0, -FutureValue, PaymentDue.EndOfPeriod);

        public int CompletedMonths => ((nextMonth.Year - StartDate.Year) * 12) + nextMonth.Month - StartDate.Month;
        public int RemainingMonths => ((SIPEndDate.Year - nextMonth.Year) * 12) + SIPEndDate.Month - nextMonth.Month + 1;

        //public double SIPAmount => Financial.Pmt(ExpectedCAGR / 12 / 100, SIPDuration * 12, 0, SIPEndFutureValue, PaymentDue.EndOfPeriod) * -1;

        public decimal RemainingInvestment => RemainingMonths * Convert.ToDecimal(SIPAmount);
        public decimal RemainingGoal => Convert.ToDecimal(FutureValue) - CurrentValue;
        

        public decimal CurrentProfit => CurrentValue - Investment;
        public decimal ExpectedProfit => Convert.ToDecimal(FutureValue) - ExpectedInvestment;
        public decimal Achieved => CurrentValue / Convert.ToDecimal(FutureValue) * 100;
        public decimal ExpectedInvestPer => ExpectedInvestment / Convert.ToDecimal(FutureValue) * 100;
        public decimal ExpectedProfitPer => ExpectedProfit / Convert.ToDecimal(FutureValue) * 100;

        public decimal GoalInvestmentPer => Investment / Convert.ToDecimal(FutureValue) * 100;
        public decimal GoalProfitPer => CurrentProfit / Convert.ToDecimal(FutureValue) * 100;
        public decimal GoalRemainingPer => 100 - GoalInvestmentPer - GoalProfitPer;

        public double CurrentCAGR { get; set; }

        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class GoalAllocation
    {
        public Goal GoalInfo { get; set; }
        public List<GoalFolioDetails> FolioDetails { get; set; }
    }

    public class GoalFolioDetails
    {
        public int GoalAllocationID { get; set; }
        public string FolioNumber { get; set; }
        public decimal Percent { get; set; }
        public bool IsActive { get; set; }
        public decimal ProfitPer { get; set; }
        public decimal XIRR { get; set; }
    }

    public class MonthView
    {
        public DateTime Date { get; set; }
        public decimal SIPAmount { get; set; }
        public decimal Investment { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Profit => CurrentValue - Investment;
        public decimal ProfitPer => Profit * 100 / Investment;
        public decimal XIRR { get; set; }
    }

    public class MonthlyGoalView : MonthView
    {
        public int GoalID { get; set; }
        public string GoalName { get; set; }

        public int PortfolioID { get; set; }
        public string Portfolio { get; set; }
        public decimal SIP { get; set; }
    }

    public class CurrentMonthTracker
    {
        public string Portfolio { get; set; }
        public string Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double CurrentGoal { get; set; }
        public double TargetGoal { get; set; }
        public List<MonthlySchemaTracker> MonthlySchemaTracker { get; set; }

        public decimal MonthlyTarget { get; set; }
        public decimal MonthInvestment => (MonthlySchemaTracker == null || MonthlySchemaTracker.Count == 0) ? 0 : MonthlySchemaTracker.Where(t => t.IsActive).Sum(t => t.CurrentMonthInvestment);
        //public decimal MonthRemaining => (MonthlySchemaTracker == null || MonthlySchemaTracker.Count == 0) ? 0 : MonthlySchemaTracker.Sum(t => t.RemainingAmount);

        public decimal InvestmentPer => MonthInvestment == 0 ? 0 : MonthInvestment * 100 / MonthlyTarget;
        public decimal MonthRemaining => MonthlyTarget - MonthInvestment;
        public decimal RemainingPer => MonthRemaining <= 0 ? 0 : MonthRemaining * 100 / MonthlyTarget;

        //public string goalFolioNumber { get; set; }
        //public string FolioNumber { get; set; }
        //public double Percent { get; set; }
        //public int SchemaCode { get; set; }
        //public string SchemaName { get; set; }
        //public string ISIN { get; set; }
        //public double MonthlyTarget { get; set; }
        //public double CurrentMonthInvestment { get; set; }

        //public double Remaining => MonthlyTarget - CurrentMonthInvestment;
        //public double RemainingPer => MonthlyTarget == 0 ? 0 : Remaining * 100 / MonthlyTarget;
    }

    public class MonthlySchemaTracker
    {
        public string FolioNumber { get; set; }
        public int SchemaCode { get; set; }
        public string SchemaName { get; set; }
        public string Category { get; set; }
        public string ISIN { get; set; }
        public decimal MonthlyTargetAmount { get; set; }
        //public decimal CurrentMonthInvestment { get; set; }
        public bool IsActive { get; set; }

        //public decimal InvestmentPer => CurrentMonthInvestment == 0 ? 0 : CurrentMonthInvestment * 100 / MonthlyTargetAmount;
        //public decimal CurrentMonthRemainingAmount => MonthlyTargetAmount - CurrentMonthInvestment;
        //public decimal CurrentMonthRemainingPer => CurrentMonthRemainingAmount <= 0 ? 0 : CurrentMonthRemainingAmount * 100 / MonthlyTargetAmount;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private DateTime nextMonth = DateTime.Now.AddMonths(1);
        private int CompletedMonths => (((nextMonth >= EndDate ? EndDate : nextMonth).Year - StartDate.Year) * 12) + (nextMonth >= EndDate ? EndDate : nextMonth).Month - StartDate.Month;

        //public MonthlyTracker CurrentMonth { get; set; }
        //public MonthlyTracker Overall { get; set; }

        public decimal OverallInvestment { get; set; }
        public decimal OverallTarget => CompletedMonths * MonthlyTargetAmount;
        public decimal OverallRemaining => OverallTarget - OverallInvestment;
        public decimal OverallInvestPer => OverallInvestment * 100 / OverallTarget;
        public decimal OverallRemainingPer => OverallRemaining * 100 / OverallTarget;


        public decimal CurrentMonthInvestment { get; set; }
        public decimal CurrentMonthRemaining => MonthlyTargetAmount - CurrentMonthInvestment;
        public decimal CurrentMonthInvestPer => CurrentMonthInvestment * 100 / MonthlyTargetAmount;
        public decimal CurrentMonthRemainingPer => CurrentMonthRemaining * 100 / MonthlyTargetAmount;
    }

    //public class MonthlyTracker
    //{
    //    public decimal Investment { get; set; }
    //    public decimal Target { get; set; }
    //    public decimal Remaining => Target - Investment;
    //    public decimal InvestPer => Investment * 100 / Target;
    //    public decimal RemainingPer => Remaining * 100 / Target;
    //}

    public class NAVDownloadDetails
    {
        public DateTime NAVDate { get; set; }
        public int Count { get; set; }
        public DateTime DownloadDateTime { get; set; }
    }
}

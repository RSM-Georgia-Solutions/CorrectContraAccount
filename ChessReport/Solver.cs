﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessReport
{
    public class Solver
    {

        public List<JournalEntryLineModel> SolveCombinations(JournalEntryLineModel targetLine, List<JournalEntryLineModel> searchLines)
        {
            _sourceLines = new List<JournalEntryLineModel>();
            RecursiveSolveCombinations(targetLine,0,new List<JournalEntryLineModel>(),searchLines,0);
            return _sourceLines;
        }

        private List<JournalEntryLineModel> _sourceLines;

        public void RecursiveSolveCombinations(JournalEntryLineModel targetLine, double currentSum, List<JournalEntryLineModel> included, List<JournalEntryLineModel> notIncluded, int startIndex)
        {
            var roundTotalsAccuracy = DiManager.Company.GetCompanyService().GetAdminInfo().TotalsAccuracy;
            for (int index = startIndex; index < notIncluded.Count; index++)
            {
                double goal = targetLine.Debit == 0 ? targetLine.Credit : targetLine.Debit;
                JournalEntryLineModel nextLine = notIncluded[index];
                double nextAmount = nextLine.Debit == 0? nextLine.Credit: nextLine.Debit;
                double amountToCompare = Math.Round(currentSum + nextAmount,roundTotalsAccuracy);

                if (amountToCompare == goal)
                {
                    List<JournalEntryLineModel> newResult = new List<JournalEntryLineModel>(included);
                    newResult.Add(nextLine);
                    _sourceLines = newResult;
                }
                else if (Math.Abs(amountToCompare) < Math.Abs(goal))
                {
                    List<JournalEntryLineModel> nextIncuded = new List<JournalEntryLineModel>(included);
                    nextIncuded.Add(nextLine);
                    List<JournalEntryLineModel> nextNonIncluded = new List<JournalEntryLineModel>(notIncluded);
                    nextNonIncluded.Remove(nextLine);
                    RecursiveSolveCombinations(targetLine, amountToCompare, nextIncuded, nextNonIncluded, startIndex++);
                }
            }
        }


        private List<Dictionary<int, double>> _mResults;
        public List<Dictionary<int, double>> Solve(double goal, Dictionary<int, double> elements)
        {
            _mResults = new List<Dictionary<int, double>>();
            RecursiveSolve(goal, 0, new Dictionary<int, double>(), new Dictionary<int, double>(elements), 0);
            return _mResults;
        }

        private void RecursiveSolve(double goal, double currentSum,
            Dictionary<int, double> included, Dictionary<int, double> notIncluded, int startIndex)
        {
            var roundTotalsAccuracy = DiManager.Company.GetCompanyService().GetAdminInfo().TotalsAccuracy;

            for (int index = startIndex; index < notIncluded.Count; index++)
            {
                double nextValue = notIncluded.Values.ElementAt(index);
                double coparerSum = Math.Round(currentSum + nextValue, roundTotalsAccuracy);
                if (coparerSum == goal)
                {
                    Dictionary<int, double> newResult = new Dictionary<int, double>(included) { { notIncluded.First().Key, nextValue } };
                    _mResults.Add(newResult);
                }
                else if (currentSum + nextValue < goal)
                {
                    Dictionary<int, double> nextIncluded = new Dictionary<int, double>(included);
                    nextIncluded.Add(notIncluded.First().Key, nextValue);
                    Dictionary<int, double> nextNotIncluded = new Dictionary<int, double>(notIncluded);
                    nextNotIncluded.Remove(notIncluded.First().Key);
                    RecursiveSolve(goal, currentSum + nextValue,
                        nextIncluded, nextNotIncluded, startIndex++);
                }
            }
        }
    }
}
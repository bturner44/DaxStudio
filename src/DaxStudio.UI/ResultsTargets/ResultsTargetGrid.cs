﻿using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DaxStudio.Interfaces;
using DaxStudio.UI.Events;
using System.Diagnostics;
using Caliburn.Micro;
using DaxStudio.UI.Interfaces;
using Serilog;
using DaxStudio.UI.Extensions;
using System.Data;

namespace DaxStudio.UI.Model
{
    // This is the default target which writes the results out to
    // the built-in grid
    [Export(typeof(IResultsTarget))]
    public class ResultsTargetGrid: IResultsTarget 
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IGlobalOptions _options;

        [ImportingConstructor]
        public ResultsTargetGrid(IEventAggregator eventAggregator, IGlobalOptions options)
        {
            _eventAggregator = eventAggregator;
            _options = options;
        }
        public string Name {get { return "Grid"; }
        }
        public string Group {get { return "Standard"; }
        }

         
        public int DisplayOrder
        {
            get { return 10; }
        }

        public Task OutputResultsAsync(IQueryRunner runner)
        {
            // Read the AutoFormat option from the options singleton
            bool autoFormat = _options.ResultAutoFormat;
            return Task.Run(() =>
                {
                    long durationMs = 0;
                    int queryCnt = 1;
                    try
                    {
                        runner.OutputMessage("Query Started");
                        var sw = Stopwatch.StartNew();

                        var dq = runner.QueryText;
                        //var res = runner.ExecuteDataTableQuery(dq);
                        using (var dataReader = runner.ExecuteDataReaderQuery(dq))
                        {
                            if (dataReader != null)
                            {
                                Log.Verbose("Start Processing Grid DataReader (Elapsed: {elapsed})" , sw.ElapsedMilliseconds);
                                runner.ResultsDataSet = dataReader.ConvertToDataSet(autoFormat);
                                Log.Verbose("End Processing Grid DataReader (Elapsed: {elapsed})", sw.ElapsedMilliseconds);

                                sw.Stop();
                                durationMs = sw.ElapsedMilliseconds;
                                var rowCnt = runner.ResultsDataSet.Tables[0].Rows.Count;
                                foreach (DataTable tbl in runner.ResultsDataSet.Tables)
                                {
                                    runner.OutputMessage(
                                        string.Format("Query {2} Completed ({0:N0} row{1} returned)", tbl.Rows.Count,
                                                        tbl.Rows.Count == 1 ? "" : "s", queryCnt));
                                    queryCnt++;
                                }
                                runner.RowCount = rowCnt;
                                // activate the result only when Counters are not selected...
                                runner.ActivateResults();
                                runner.OutputMessage("Query Batch Completed", durationMs);
                            }
                            else
                                runner.OutputError("Query Batch Completed with errors", durationMs);

                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Log.Error("{class} {method} {message} {stacktrace}", "ResultsTargetGrid","OutputQueryResultsAsync",ex.Message, ex.StackTrace);
                        runner.ActivateOutput();
                        runner.OutputError(ex.Message);
                        runner.OutputError("Query Batch Completed with erros", durationMs);
                    }
                    finally
                    {
                        
                        runner.QueryCompleted();
                    }
                });
        }

        public bool IsDefault
        {
            get { return true; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }


        public string Message
        {
            get { return string.Empty;}
        }
        public OutputTargets Icon
        {
            get { return OutputTargets.Grid; }
        }
    }


}

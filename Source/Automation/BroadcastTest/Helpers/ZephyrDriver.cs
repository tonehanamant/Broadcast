using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BroadcastTest.Utils
{
    public class ZephyrDriver
    {


        public int GetJiraProjectIdFromCode(string code)
        {
           //Code = BB, BCOP, etc used in jira id
           switch (code.ToUpper())
           {
               case "BB":
                   return 1;
               case "BCOP":
                   return 2;
               default :
                   return 0;
           }  
            //TODO: Replace with App.Config lookup
        }
        
        public TestStep GetSpecFlowScenarioStepTagInfo(string tag)
        {
            TestStep step = new TestStep();
            //Parse following format example: @AUTOTEST_BCOP164_STEP4
            if (tag.ToUpper().Contains("AUTOTEST"))
            {                
                string[] stepInfo = Regex.Split(tag, "_");
                
                string[] jiraId = Regex.Split(stepInfo[1], "(?<=\\d)(?=\\D)|(?=\\d)(?<=\\D)");
                string projectCode = jiraId[0];
                string testId = jiraId[1];

                string[] stepinfo = Regex.Split(stepInfo[2], "(?<=\\d)(?=\\D)|(?=\\d)(?<=\\D)");
                string stepNum = stepInfo[1];
                

                //Parse these strings
                step.ProjectId = GetJiraProjectIdFromCode(projectCode);
                step.StepId = Convert.ToInt16(stepNum);
                step.IssueId = Convert.ToInt16(testId);
                
            }

            return step;
        }


        //Indicate testing started on test using test case ticket number and test cycle.
        public void CreateTestExecution(TestCaseExecution exec) { 

            string requestUrl = "http://localhost:2990/jira/rest/zapi/latest/execution";

            string body = JsonConvert.SerializeObject(exec); 

        }

        public void UpdateTestExecution(int execId, int statusId)
        {

            string requestUrl = "http://localhost:2990/jira/rest/zapi/latest/execution/" + execId.ToString() + "/execute";
            string body = " { \"status\": " + statusId + "\" }";
        }

        //Indicate testing stopped on test using test case ticket number.
        public void StopTestExecution(string zephyrId, string cycleName) 
        {
            
        }


        public void SetTestExecutionStatus(string testId, string cycleId)
        {

        }




        public void LogStepResult(string zephyrId, string cycleName, int stepId, string status) {

            string requestUrl = "http://localhost:2990/jira/rest/zapi/latest/stepResult";

            TestStep step = new TestStep();
            step.Status = 1;
            step.IssueId = 1;
            step.StepId = 1;


            string jsonBody = JsonConvert.SerializeObject(step); 
        
        }


        public class TestCase
        {

            public int ProjectId { get; set; }
            public int VersionId { get; set; }
            public int GroupFld { get; set; }

            public int Status { get; set; }
            public int GetStatus(string statusMsg)
            {
                switch (statusMsg.ToUpper())
                {
                    case "PASS":
                        return 1;
                    case "FAIL":
                        return 2;
                    case "WIP":
                        return 3;
                    case "BLOCKED":
                        return 4;
                    case "UNEXECUTED":
                        return 5;
                    default:
                        return 0;
                }
            }
            public void SetStatus(string statusMsg)
            {
                this.Status = GetStatus(statusMsg);
            }

        }

        public class TestCaseExecution
        {
            public int IssueId { get; set; }
            public long ProjectId { get; set; }
            public long VersionId { get; set; }
            public int CycleId { get; set; }
   
            //Params used for Creation
            public string AssigneeType {get; set;}
            public string Assignee { get; set; }

            //Used for retrieving list results?
            public int offset { get; set; } //??? 
            public string Action { get; set; }
            public string Sorter { get; set; }
            public string Expand { get; set; }
            public int limit { get; set; }
        }

        public class TestStep
        {
            public int StepId { get; set; }
            public long ProjectId { get; set; }
            public int IssueId { get; set; }
            public int ExecutionId { get; set; }
            public int Status { get; set; }

            public int GetStatus(string statusMsg)
            {
                switch (statusMsg.ToUpper())
                {
                    case "PASS":
                        return 1;
                    case "FAIL":
                        return 2;
                    case "WIP":
                        return 3;
                    case "BLOCKED":
                        return 4;
                    case "UNEXECUTED":
                        return 5;
                    default:
                        return 0;
                }
            }

            public void SetStatus(string statusMsg)
            {
                this.Status = GetStatus(statusMsg);
            }

        }


        public class TestStepStatus
        {
            public string Pass = "PASS";
            public string Fail = "FAIL";
            public string WIP = "WIP";
            public string Blocked = "BLOCKED";
            public string Unexecuted = "UNEXECUTED";



        }

        public void GetTestSummary(string zephyrId) { }
        public void GetTestVersions(string zephyrId) { }
        public void GetLatestTestVersion(string zephyrId) { }

        public void GetTestCycles(string zephyrId) { }
        public void GetLatestTestCycle(string zephyrId) { }

        public void GetTestPriority(string zephyrId) { }

    }
}

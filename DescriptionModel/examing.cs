using System;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// 描述考试系统
/// </summary>
namespace DescriptionModel.examing {
    public class Subject {
        public int Id { get; set; }
        public string Description { get; set; }
        /// <summary>1,单选，2，多选，3 填空，4 判断(相当于选择)，5 连线(可转化为选择形式)，6 问答(可转化为一组相关的填空题),7应用(可转化为一组相关的填空题)</summary>
        public string Type { get; set; }
        /// <summary>是否需要计算</summary>
        public bool? IsNeedCompute { get; set; }
        /// <summary>回答正确得分</summary>
        public float? Score { get; set; }
        /// <summary>解答错误需要扣除的分数</summary>
        public float? UnScore { get; set; }
        public DateTime? PublishDate { get; set; }
        /// <summary>源自于，包括 原创</summary>
        public string From { get; set; }
        /// <summary>领域，根据领域年代可以组成试卷</summary>
        public int? DomainId { get; set; }
    }
    public struct SubjectType {
        public string GapFilling  => nameof(GapFilling);
        public string SingleSelect => nameof(SingleSelect);
        public string MultiSelect => nameof(MultiSelect);
        public string JudgeBySingleSelect => nameof(JudgeBySingleSelect);
        public string QuestionAnwserByGapFilling => nameof(QuestionAnwserByGapFilling);
    }
    public class Answer {
        public int Id { get; set; }
        public int? SubjectId { get; set; }
        public string Result { get; set; }
        public double ResultValue {
            get {
                double.TryParse(this.Result, out double x);
                return x;
            }
        }
    }
    public class TestPaper {
        public int Id { get; set; }
        public int? Pid { get; set; }
        public string Title { get; set; }
        public int? Time { get; set; }
        public DateTime? Date { get; set; }
        public int? DomainId { get; set; }
    }
    public class TestPaper_Subject {
        public int Id { get; set; }
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
    }
}

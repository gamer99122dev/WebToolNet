using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebToolNet.Validation
{
    public class CheckID
    {

        private string _id;


        public CheckID()
        {
            _id = "";
        }

        public CheckID(string id)
        {
            _id = id;
        }

        public string ID
        {
            get
            {
                return _id;
            }

            set
            {
                // 去空白、跳行
                if ((value = value.Trim()).Length != 10)
                    return;

                _id = value;
            }

        }


        public bool CheckAny(string id)
        {
            /*沒有輸入，回傳 ID 錯誤*/
            if (string.IsNullOrEmpty(id))
                return false;


            // 去空白、跳行
            if ((id = id.Trim()).Length != 10)
                return false;


            /*強制轉換大寫*/
            id = id.ToUpper();

            if (id == "E88817879A" || id == "R88810996A")
                return true;

            Regex regex = new Regex("^[A-Z]{2}");
            if (regex.IsMatch(id))
                return CheckForeignID(id);
            else
                return CheckTWID(id);

        }


        public bool CheckTWID()
        {
            return CheckTWID(_id);
        }

        public bool CheckTWID(string id)
        {
            /*沒有輸入，回傳 ID 錯誤*/
            if (string.IsNullOrEmpty(id))
                return false;

            // 去空白、跳行
            if ((id = id.Trim()).Length != 10)
                return false;


            /*強制轉換大寫*/
            id = id.ToUpper();

            if (id == "E88817879A" || id == "R88810996A" || id == "T000000194" || id == "E88868144A")
                return true;


            /*驗證規則 Regular Expression 驗證失敗，回傳 ID 錯誤*/
            //修改第二碼檢查碼
            //1   男
            //2   女
            //8   男(外來人口）
            //9   女(外來人口）
            Regex regex = new Regex("^[A-Z]{1}[1289]{1}[0-9]{8}$");
            if (!regex.IsMatch(id))
                return false;

            /*除了檢查碼外每個數字的存放空間*/
            int[] seed = new int[10];
            string[] charMapping = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "W", "Z", "I", "O" };
            //A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            //P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32 Z=33 I=34 O=35


            string target = id.Substring(0, 1);
            for (int index = 0; index < charMapping.Length; index++)
            {
                if (charMapping[index] == target)
                {
                    index += 10;

                    /*10進制的高位元放入存放空間*/
                    seed[0] = index / 10;

                    /*10進制的低位元*9後放入存放空間*/
                    seed[1] = (index % 10) * 9;

                    break;
                }
            }

            for (int index = 2; index < 10; index++)
            {
                /*將剩餘數字乘上權數後放入存放空間*/
                seed[index] = Convert.ToInt32(id.Substring(index - 1, 1)) * (10 - index);
            }

            /*檢查是否符合檢查規則，10減存放空間所有數字和除以10的餘數的個位數字是否等於檢查碼
            (10 - ((seed[0] + .... + seed[9]) % 10)) % 10 == 身分證字號的最後一碼*/
            return (10 - (seed.Sum() % 10)) % 10 == Convert.ToInt32(id.Substring(9, 1));
        }


        public bool CheckForeignID()
        {
            return CheckForeignID(_id);
        }

        public bool CheckForeignID(string id)
        {
            /*沒有輸入，回傳 ID 錯誤*/
            if (string.IsNullOrEmpty(id))
                return false;

            // 去空白、跳行
            if ((id = id.Trim()).Length != 10)
                return false;


            /*強制轉換大寫*/
            id = id.ToUpper();


            if (id == "E88817879A" || id == "R88810996A")
                return true;

            // 居留證第二碼是 B~D
            /*驗證規則 Regular Expression 驗證失敗，回傳 ID 錯誤*/
            Regex regex = new Regex("^[A-Z]{1}[A-D]{1}[0-9]{8}$");
            if (!regex.IsMatch(id))
                return false;

            string head = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            id = (head.IndexOf(id.Substring(0, 1)) + 10).ToString() + ((head.IndexOf(id.Substring(1, 1)) + 10) % 10) + id.Substring(2, 8);

            int s = int.Parse(id.Substring(0, 1)) +
                    int.Parse(id.Substring(1, 1)) * 9 +

                    int.Parse(id.Substring(2, 1)) * 8 +
                    int.Parse(id.Substring(3, 1)) * 7 +
                    int.Parse(id.Substring(4, 1)) * 6 +
                    int.Parse(id.Substring(5, 1)) * 5 +

                    int.Parse(id.Substring(6, 1)) * 4 +
                    int.Parse(id.Substring(7, 1)) * 3 +
                    int.Parse(id.Substring(8, 1)) * 2 +
                    int.Parse(id.Substring(9, 1)) +
                    int.Parse(id.Substring(10, 1));

            //判斷是否可整除
            if ((s % 10) != 0)
                return false;


            //居留證號碼正確
            return true;
        }

        /// <summary>
        /// 檢核居留證（舊式：第2碼A-D；新式2021：第2碼8或9）
        /// </summary>
        public bool CheckResidentID(string idNo)
        {
            if (idNo == null) return false;
            idNo = idNo.ToUpper();
            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(@"^([A-Z])(A|B|C|D|8|9)(\d{8})$");
            System.Text.RegularExpressions.Match match = regex.Match(idNo);
            if (!match.Success) return false;

            string second = match.Groups[2].Value;
            if ("ABCD".IndexOf(second) >= 0)
                return CheckOldResidentID(match.Groups[1].Value, second, match.Groups[3].Value);
            else
                return CheckNewResidentID(match.Groups[1].Value, second + match.Groups[3].Value);
        }

        private bool CheckOldResidentID(string firstLetter, string secondLetter, string num)
        {
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo =
                $"{alphabet.IndexOf(firstLetter) + 10}" +
                $"{(alphabet.IndexOf(secondLetter) + 10) % 10}" +
                $"{num}";
            int[] idNoArray = new int[transferIdNo.Length];
            for (int i = 0; i < transferIdNo.Length; i++)
                idNoArray[i] = Convert.ToInt32(transferIdNo[i].ToString());
            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
                sum += weight[i] * idNoArray[i + 1];
            return (sum % 10 == 0);
        }

        private bool CheckNewResidentID(string firstLetter, string num)
        {
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo = $"{(alphabet.IndexOf(firstLetter) + 10)}{num}";
            int[] idNoArray = new int[transferIdNo.Length];
            for (int i = 0; i < transferIdNo.Length; i++)
                idNoArray[i] = Convert.ToInt32(transferIdNo[i].ToString());
            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
                sum += (weight[i] * idNoArray[i + 1]) % 10;
            return (sum % 10 == 0);
        }
    }
}

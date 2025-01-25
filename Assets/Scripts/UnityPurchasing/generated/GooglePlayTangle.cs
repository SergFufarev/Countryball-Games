// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("sTI8MwOxMjkxsTIyM5+Sd+lzUzyB5HFNCPuMz90N5ejnJmGzf+VnBr/0Rmv3WQW/8VvKMSn6ua3BZJ4SJhvBeyLHUt0N9Xk/ki8fboXrH0mKMVezmQBOp6vCTLT2BVFd+xWEMWqyhMsUuCkAYoCeGRpY+jo71YQosxqiVPtMAzy/equtwTYGItY4j7K+5rqjaDL7H2DJWMJeHw6+touOJwOxMhEDPjU6GbV7tcQ+MjIyNjMwUCi1Za50SLd7xx5wW9+GW+Q03rCue/qkUABYrXFXcsb0hVnG9C9TZpsx2Om6S5Cl9liEKfrbcJ63fvxPuS838+SZYYHKLxZtuYUTJypAC6VE5eKFQGrp2io7bPxa8nBzswGAufyA0aCFso/7jDEwMjMy");
        private static int[] order = new int[] { 1,4,13,13,7,9,10,13,13,13,11,11,13,13,14 };
        private static int key = 51;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

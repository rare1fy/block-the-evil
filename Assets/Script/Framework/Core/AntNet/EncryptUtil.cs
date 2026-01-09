using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AntNet
{
    public class EncryptUtil
    {
        private static string strEncryptKey;
        private static byte[] bytesEncryptKey;
        private static bool openEncrypt = false;
        private static Dictionary<int, List<int>> decryptCmdActExclude = new Dictionary<int, List<int>>();
        private static Dictionary<int, List<int>> encryptCmdActExclude = new Dictionary<int, List<int>>();
        
        public static void RegEvent()
        {
            //EventDispatchCenter.Instance.Registry(SDEvents.L2C_UPDATE_ENCRYPT_KEY, OnUpdateEncryptKey);
            //EventDispatchCenter.Instance.Registry(SDEvents.L2C_UPDATE_NOT_DECRYPT_CMD_ACT, UpdateDecryptCmdActExclude);
            //EventDispatchCenter.Instance.Registry(SDEvents.L2C_UPDATE_NOT_ENCRYPT_CMD_ACT, UpdateEncryptCmdActExclude);
        }

        public static void UnRegEvent()
        {
            //EventDispatchCenter.Instance.UnRegistry(SDEvents.L2C_UPDATE_ENCRYPT_KEY, OnUpdateEncryptKey);
            //EventDispatchCenter.Instance.UnRegistry(SDEvents.L2C_UPDATE_NOT_DECRYPT_CMD_ACT, UpdateDecryptCmdActExclude);
            //EventDispatchCenter.Instance.UnRegistry(SDEvents.L2C_UPDATE_NOT_ENCRYPT_CMD_ACT, UpdateEncryptCmdActExclude);
        }
        private static void UpdateCmdActDic(object o, bool encrypt)
        {
            var dic = encrypt ? encryptCmdActExclude : decryptCmdActExclude;
            //XLua.LuaTable table = o as XLua.LuaTable;
            //if (null != table)
            //{
            //    for (int i = 1, iLen = table.Length; i <= iLen; i++)
            //    {
            //        XLua.LuaTable subTable = null;
            //        table.Get(i, out subTable);
            //        if (null != subTable)
            //        {
            //            int cmd = -1;
            //            int act = -1;
            //            subTable.Get(1, out cmd);
            //            subTable.Get(2, out act);
            //            List<int> list = null;
            //            if(!dic.TryGetValue(cmd, out list))
            //            {
            //                list = new List<int>();
            //                dic.Add(cmd, list);
            //            }
            //            //Debug.Log((encrypt ? "不加密：" : "不解密：") + " cmd : " + cmd + " act : " + act);
            //            list.Add(act);
            //        }
            //    }
            //}
        }
        private static void UpdateDecryptCmdActExclude(object o)
        {
            UpdateCmdActDic(o, false);
        }
        private static void UpdateEncryptCmdActExclude(object o)
        {
            UpdateCmdActDic(o, true);
        }
        private static bool NeedEnDecrypt(int cmd, int act, bool encrypt)
        {
            var dic = encrypt ? encryptCmdActExclude : decryptCmdActExclude;
            List<int> list = null;
            if(null != dic && dic.Count > 0 && dic.TryGetValue(cmd, out list) && list.Contains(act))
                return false;
            return true;
        }
        private static void OnUpdateEncryptKey(object o)
        {
            string strKey = o as string;
            openEncrypt = !string.IsNullOrEmpty(strKey);
            Debug.Log("openEncrypt : " + openEncrypt);
            if (openEncrypt)
            {
                strEncryptKey = (string)o;
                bytesEncryptKey = Encoding.Default.GetBytes(strEncryptKey);
            }
        }

        /// <summary>
        /// 对网络消息加密
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="act"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encrypt(int cmd, int act,byte[] data)
        {
            if (!openEncrypt)
                return data;
            if (data.Length > 0 && NeedEnDecrypt(cmd, act, true))
            {
                //Debug.Log(cmd + "  " + act + "  加密");
                return Encrypt(data, bytesEncryptKey, bytesEncryptKey);
            }else
            {
                //Debug.Log(cmd + "  " + act + "  不加密");
            }
                
            return data;
        }
        public static byte[] Decrypt(int cmd, int act, byte[] data)
        {
            if (!openEncrypt)
                return data;
            if (null == data || data.Length < bytesEncryptKey.Length || !NeedEnDecrypt(cmd, act,false))
                return data;
            byte[] decryptData = null;
            //如果解密失败，则直接返回原始数据
            try
            {
                //Debug.Log(cmd + "  " + act + "  解密");
                decryptData = Decrypt(data, bytesEncryptKey, bytesEncryptKey);
            }
            catch (Exception)
            {
                decryptData = data;
            }
            return decryptData;
        }

        private static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var crypto = aes.CreateEncryptor(key, iv);
            byte[] decrypted = crypto.TransformFinalBlock(
                    data, 0, data.Length);

            crypto.Dispose();

            return decrypted;
        }

        private static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var dcrypto = aes.CreateDecryptor(key, iv);
            byte[] decrypted = dcrypto.TransformFinalBlock(data, 0, data.Length);

            dcrypto.Dispose();

            return decrypted;
        }

        public static int UintToInt(uint o)
        {
            int i = 0;
            checked
            {
                i = (int)o;
            }
            return i;
        }

    }

}

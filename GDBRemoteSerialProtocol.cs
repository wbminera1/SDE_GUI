using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDE_GUI
{
    class GDBRemoteSerialProtocol
    {

        enum eCommands
        {
            eNone,
            /*
             */
            qSupported,
            /* ‘!’ Enable extended mode. In extended mode, the remote server is made persistent. 
            * The ‘R’ packet is used to restart the program being debugged.
            * Reply: ‘OK’ The remote target both supports and has enabled extended mode.*/
            eExtendedMode,
            /* ‘?’
            * Indicate the reason the target halted. The reply is the same as for step and continue. This packet has a special interpretation when the target is in non-stop mode; see Remote Non-Stop.
            * Reply: See Stop Reply Packets, for the reply specifications.*/
            eReason,
            /* ‘A arglen, argnum, arg,…’
            * Initialized argv[]
            * array passed into program. arglen specifies the number of bytes in the hex encoded byte stream arg.See gdbserver for more details.
            * Reply: ‘OK’ The arguments were set. ‘E NN’ An error occurred. */
            eArguments,
            /* ‘bc’
            * Backward continue. Execute the target system in reverse. No parameter. See Reverse Execution, for more information.
            * Reply: See Stop Reply Packets, for the reply specifications.*/
            eBackwardContinue,
            /* ‘bs’
            * Backward single step. Execute one instruction in reverse. No parameter. See Reverse Execution, for more information.
            * Reply: See Stop Reply Packets, for the reply specifications.*/
            eBackwardSingleStep,
            /* ‘g’
            * Read general registers.
            * Reply: ‘XX…’ Each byte of register data is described by two hex digits.The bytes with the register are transmitted in target byte order.The size of each register and their position within the ‘g’ packet are determined by the GDB internal gdbarch functions DEPRECATED_REGISTER_RAW_SIZE and gdbarch_register_name.
            * When reading registers from a trace frame(see Using the Collected Data), the stub may also return a string of literal ‘x’’s in place of the register data digits, to indicate that the corresponding register has not been collected, thus its value is unavailable.For example, for an architecture with 4 registers of 4 bytes each, the following reply indicates to GDB that registers 0 and 2 have not been collected, while registers 1 and 3 have been collected, and both have zero value:
            * -> g <- xxxxxxxx00000000xxxxxxxx00000000
            * ‘E NN’ for an error.*/
            eRegisters,
            /*
             * */
            eRegister,

            /* ‘i [addr[, nnn]]’
            * Step the remote target by a single clock cycle. If ‘, nnn’ is present, cycle step nnn cycles. If addr is present, cycle step starting at that address.*/
            eStepSingle,

            eLast
        }

        eCommands m_State = eCommands.eNone;
        bool    m_Confirmed = false;

        static public byte[] MakeCommand(string str)
        {
            byte[] cmd = new byte[1 + str.Length + 1 + 2];
            int idx = 0;
            cmd[idx++] = (byte)'$';
            byte summ = 0;
            foreach(char c in str)
            {
                cmd[idx++] = (byte)c;
                summ += (byte)c;
            }
            cmd[idx++] = (byte)'#';
            cmd[idx++] = (byte)(48 + ((summ >> 4) & 0x0F));
            cmd[idx++] = (byte)(48 + (summ & 0x0F));
            return cmd;
        }
        static public string ParseCommand(byte[] cmd)
        {
            string res = "";
            byte count = 0;
            for(int idx = 0; idx < cmd.Length; ++idx)
            {
                byte b = cmd[idx];
                if(b == '#') {
                    break;
                }
                if(b != '$') {
                    if(b == '*') {
                        byte chr = cmd[idx - 1];
                        byte chrCount = cmd[idx + 1];
                        chrCount -= 29;
                        for(byte chrIdx = 0; chrIdx < chrCount; ++chrIdx) {
                            ++count;
                            res += (char)chr;
                        }
                        ++idx;
                        continue;
                    }
                    ++count;
                    res += (char)b;
                }
            }
            return res;
        }

        public byte[] qSupported()
        {
            m_State = eCommands.qSupported;
            m_Confirmed = false;
            return MakeCommand("qSupported");
        }

        public byte[] eExtendedMode()
        {
            m_State = eCommands.eExtendedMode;
            m_Confirmed = false;
            return MakeCommand("eExtendedMode");
        }
        public byte[] eRegisters()
        {
            m_State = eCommands.eRegisters;
            m_Confirmed = false;
            return MakeCommand("g");
        }

        public byte[] eRegister(string register)
        {
            m_State = eCommands.eRegister;
            m_Confirmed = false;
            return MakeCommand("p " + register);
        }

        public byte[] eStepSingle()
        {
            m_State = eCommands.eStepSingle;
            m_Confirmed = false;
            return MakeCommand("i");
        }

        public byte[] eReason()
        {
            m_State = eCommands.eReason;
            m_Confirmed = false;
            return MakeCommand("vStopped");
        }

        public byte[] DataReceiveCallback(byte[] result, int bytesRead)
        {
            if(m_State != eCommands.eNone)
            {
                if(result[0] == '+')
                {
                    m_Confirmed = true;
                    return null;
                }
            }
            if (result[0] == '$')
            {
                ParseCommand(result);
                byte[] res = new byte[1];
                res[0] = (byte)'+';
                return res;
            }
            return null;
        }
    }
}

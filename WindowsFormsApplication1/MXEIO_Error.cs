using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MOXA_CSharp_MXIO;

namespace WindowsFormsApplication1
{
    class MXEIO_Error
    {
        public static string CheckErr(int iRet, string szFunctionName)
        {
            string szErrMsg = "MXIO_OK";

            if (iRet != MXIO_CS.MXIO_OK)
            {

                switch (iRet)
                {
                    case MXIO_CS.ILLEGAL_FUNCTION:
                        szErrMsg = "ILLEGAL_FUNCTION";
                        break;
                    case MXIO_CS.ILLEGAL_DATA_ADDRESS:
                        szErrMsg = "ILLEGAL_DATA_ADDRESS";
                        break;
                    case MXIO_CS.ILLEGAL_DATA_VALUE:
                        szErrMsg = "ILLEGAL_DATA_VALUE";
                        break;
                    case MXIO_CS.SLAVE_DEVICE_FAILURE:
                        szErrMsg = "SLAVE_DEVICE_FAILURE";
                        break;
                    case MXIO_CS.SLAVE_DEVICE_BUSY:
                        szErrMsg = "SLAVE_DEVICE_BUSY";
                        break;
                    case MXIO_CS.EIO_TIME_OUT:
                        szErrMsg = "EIO_TIME_OUT";
                        break;
                    case MXIO_CS.EIO_INIT_SOCKETS_FAIL:
                        szErrMsg = "EIO_INIT_SOCKETS_FAIL";
                        break;
                    case MXIO_CS.EIO_CREATING_SOCKET_ERROR:
                        szErrMsg = "EIO_CREATING_SOCKET_ERROR";
                        break;
                    case MXIO_CS.EIO_RESPONSE_BAD:
                        szErrMsg = "EIO_RESPONSE_BAD";
                        break;
                    case MXIO_CS.EIO_SOCKET_DISCONNECT:
                        szErrMsg = "EIO_SOCKET_DISCONNECT";
                        break;
                    case MXIO_CS.PROTOCOL_TYPE_ERROR:
                        szErrMsg = "PROTOCOL_TYPE_ERROR";
                        break;
                    case MXIO_CS.SIO_OPEN_FAIL:
                        szErrMsg = "SIO_OPEN_FAIL";
                        break;
                    case MXIO_CS.SIO_TIME_OUT:
                        szErrMsg = "SIO_TIME_OUT";
                        break;
                    case MXIO_CS.SIO_CLOSE_FAIL:
                        szErrMsg = "SIO_CLOSE_FAIL";
                        break;
                    case MXIO_CS.SIO_PURGE_COMM_FAIL:
                        szErrMsg = "SIO_PURGE_COMM_FAIL";
                        break;
                    case MXIO_CS.SIO_FLUSH_FILE_BUFFERS_FAIL:
                        szErrMsg = "SIO_FLUSH_FILE_BUFFERS_FAIL";
                        break;
                    case MXIO_CS.SIO_GET_COMM_STATE_FAIL:
                        szErrMsg = "SIO_GET_COMM_STATE_FAIL";
                        break;
                    case MXIO_CS.SIO_SET_COMM_STATE_FAIL:
                        szErrMsg = "SIO_SET_COMM_STATE_FAIL";
                        break;
                    case MXIO_CS.SIO_SETUP_COMM_FAIL:
                        szErrMsg = "SIO_SETUP_COMM_FAIL";
                        break;
                    case MXIO_CS.SIO_SET_COMM_TIME_OUT_FAIL:
                        szErrMsg = "SIO_SET_COMM_TIME_OUT_FAIL";
                        break;
                    case MXIO_CS.SIO_CLEAR_COMM_FAIL:
                        szErrMsg = "SIO_CLEAR_COMM_FAIL";
                        break;
                    case MXIO_CS.SIO_RESPONSE_BAD:
                        szErrMsg = "SIO_RESPONSE_BAD";
                        break;
                    case MXIO_CS.SIO_TRANSMISSION_MODE_ERROR:
                        szErrMsg = "SIO_TRANSMISSION_MODE_ERROR";
                        break;
                    case MXIO_CS.PRODUCT_NOT_SUPPORT:
                        szErrMsg = "PRODUCT_NOT_SUPPORT";
                        break;
                    case MXIO_CS.HANDLE_ERROR:
                        szErrMsg = "HANDLE_ERROR";
                        break;
                    case MXIO_CS.SLOT_OUT_OF_RANGE:
                        szErrMsg = "SLOT_OUT_OF_RANGE";
                        break;
                    case MXIO_CS.CHANNEL_OUT_OF_RANGE:
                        szErrMsg = "CHANNEL_OUT_OF_RANGE";
                        break;
                    case MXIO_CS.COIL_TYPE_ERROR:
                        szErrMsg = "COIL_TYPE_ERROR";
                        break;
                    case MXIO_CS.REGISTER_TYPE_ERROR:
                        szErrMsg = "REGISTER_TYPE_ERROR";
                        break;
                    case MXIO_CS.FUNCTION_NOT_SUPPORT:
                        szErrMsg = "FUNCTION_NOT_SUPPORT";
                        break;
                    case MXIO_CS.OUTPUT_VALUE_OUT_OF_RANGE:
                        szErrMsg = "OUTPUT_VALUE_OUT_OF_RANGE";
                        break;
                    case MXIO_CS.INPUT_VALUE_OUT_OF_RANGE:
                        szErrMsg = "INPUT_VALUE_OUT_OF_RANGE";
                        break;
                }


                if ((iRet == MXIO_CS.EIO_TIME_OUT) || (iRet == MXIO_CS.HANDLE_ERROR) || (iRet == MXIO_CS.EIO_SOCKET_DISCONNECT))
                {
                   
                }
            }
            return szErrMsg;
        }
    
    }
}

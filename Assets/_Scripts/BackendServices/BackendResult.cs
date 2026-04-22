using UnityEngine;

namespace ProgressiveP.Backend
{
  public class BackendResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        private BackendResult() { }

        public static BackendResult<T> Success(T data)
            => new BackendResult<T> { IsSuccess = true, Data = data };

        public static BackendResult<T> Failure(string errorCode, string errorMessage)
            => new BackendResult<T> { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
    }


  
    public class BackendData
    {
        public string value;
        
        public BackendData(string value)
        {
            this.value = value;
        }
    }
}

    


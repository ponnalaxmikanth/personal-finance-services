using System;
using System.Text;

namespace Entities.Models.DTO
{
    public class ServiceResponse<T>
    {
        private StringBuilder _messages;
        private StringBuilder _warnings;
        private StringBuilder _errors;

        public bool Success { get; set; }
        public T ResponseObject { get; private set; }
        public string Message => _messages.ToString();
        public string Error => _errors.ToString();
        public string Warning => _warnings.ToString();

        public ServiceResponse() {
            _messages = new StringBuilder();
            _warnings = new StringBuilder();
            _errors = new StringBuilder();
        }

        public ServiceResponse(bool success, string message, T responseObject) : this()
        {
            Success = success;
            _messages.Append(message);
            ResponseObject = responseObject;
        }

        public void SetMessage(string message) {
            _messages.Append(message);
        }

        public void SetWarning(string message) {
            _warnings.Append(message);
        }

        public void SetFailure()
        {
            Success = false;
        }

        public void SetFailure(string message) {
            SetFailure();
            _errors.Append(message);
        }
        

        public void SetSuccess()
        {
            Success = true;
        }
    }
}

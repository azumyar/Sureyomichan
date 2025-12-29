using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Haru.Kei.SureyomiChan.Exceptions;

class SureyomiChanException : Exception {
	public SureyomiChanException() { }
	public SureyomiChanException(string message) : base(message) { }
	public SureyomiChanException(string message, Exception innerException) : base(message, innerException) { }
}

class ImageNotSupportException : SureyomiChanException {}

class ApiHttpErrorException : SureyomiChanException {
	public string Url { get; }
	public HttpRequestException HttpRequestException { get; }

	public ApiHttpErrorException(string url, HttpRequestException innerException) : base() {
		this.Url = url;
		this.HttpRequestException = innerException;
	}
}

class ApiHttpConnectionException : SureyomiChanException {
	public Exception ConnectionException { get; }

	public ApiHttpConnectionException(Exception innerException) : base() {
		this.ConnectionException = innerException;
	}
}

class ApiInvalidJsonException : SureyomiChanException {
	public string Json { get; }

	public ApiInvalidJsonException(string json) : base() {
		this.Json = json;
	}
}
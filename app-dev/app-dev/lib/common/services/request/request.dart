import 'dart:io';

import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter/foundation.dart';

typedef HandleTokenExpireException = Future<void> Function(DioException e, ErrorInterceptorHandler handler);

class Request {
  final String baseUrl;
  final HandleTokenExpireException? handleTokenExpireException;

  Request({required this.baseUrl, this.handleTokenExpireException}) {
    _dio.options.connectTimeout = const Duration(seconds: 60);
    _dio.options.sendTimeout = const Duration(seconds: 60);
    _dio.options.receiveTimeout = const Duration(seconds: 60);
    _dio.options.baseUrl = baseUrl;

    _dio.interceptors.add(InterceptorsWrapper(onRequest: _onRequest, onResponse: _onResponse, onError: _onError));
    if (!kIsWeb) {
      (_dio.httpClientAdapter as IOHttpClientAdapter).createHttpClient =
          () =>
              HttpClient()
                ..badCertificateCallback = ((X509Certificate cert, String host, int port) => true)
                ..idleTimeout = const Duration(milliseconds: 1500);
    }
  }

  final _dio = Dio();
  String? _authToken;

  void setAuthToken(String token) {
    _authToken = token;
  }

  void _onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    debugPrint('send request：path:${options.path}，baseURL:${options.baseUrl}');

    if (_authToken != null) {
      options.headers['Authorization'] = 'Bearer $_authToken';
    }

    final connectivity = await checkConnectivity;
    if (connectivity) {
      return handler.next(options);
    } else {
      // showToast('أنت غير متصل بالإنترنت', type: ToastType.error);
      final e = DioException(requestOptions: options);
      return handler.reject(e, true);
    }
  }

  Future<Response> download(
    String path,
    String savePath, {
    Map<String, dynamic>? queryParameters,
    Function(int current, int total)? onReceiveProgress,
    CancelToken? cancelToken,
    Options? options,
  }) => _dio.download(path, savePath, onReceiveProgress: onReceiveProgress, cancelToken: cancelToken, options: options);

  _onResponse(Response response, ResponseInterceptorHandler handler) {
    debugPrint('received data from server: ${response.data}');
    // if (response.data['status'] != 200 && response.data['status'] != 'success') {
    //   if (response.data['message'] != null && response.data['message'].isNotEmpty) {
    //     showToast(response.data['message']);
    //   }
    // }
    // if (response.data['data'] != null) {
    //   response.data = response.data['data'];
    // }
    return handler.next(response);
  }

  void _onError(DioException e, ErrorInterceptorHandler handler) async {
    if (e.response?.statusCode == 401) {
      try {
        await handleTokenExpireException?.call(e, handler);
      } catch (_) {
        return handler.reject(e);
      }

      final cloneRequest = await _repeateRequest(e);
      return handler.resolve(cloneRequest);
    }
    // if (e.response?.data['message'] != null && e.response?.data['message'].isNotEmpty) {
    //   showToast(e.response?.data['message'], type: ToastType.error);
    // } else if (e.type == DioExceptionType.connectionTimeout) {
    //   showToast('connection time out', type: ToastType.error);
    // }

    return handler.reject(e);
  }

  Future<bool> get checkConnectivity async {
    if (!kIsWeb) {
      try {
        final result = await InternetAddress.lookup('google.com');
        if (result.isNotEmpty && result[0].rawAddress.isNotEmpty) {
          return true;
        }
      } on SocketException catch (_) {
        return false;
      }
      return false;
    } else {
      return true;
    }
  }

  Future<Response> get(String path, {Map<String, dynamic>? queryParameters}) =>
      _dio.get(path, queryParameters: queryParameters);

  Future<Response> post(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Function(int current, int total)? onSendProgress,
    CancelToken? cancelToken,
  }) => _dio.post(
    path,
    data: data,
    queryParameters: queryParameters,
    onSendProgress: onSendProgress,
    cancelToken: cancelToken,
    options: Options(contentType: 'application/json'),
  );

  Future<Response> put(String path, {dynamic data, Map<String, dynamic>? queryParameters}) =>
      _dio.put(path, data: data, queryParameters: queryParameters);

  Future<Response> patch(String path, {dynamic data, Map<String, dynamic>? queryParameters}) =>
      _dio.patch(path, data: data, queryParameters: queryParameters);

  Future<Response> delete(String path, {dynamic data, Map<String, dynamic>? queryParameters}) =>
      _dio.delete(path, queryParameters: queryParameters, data: data);

  Future<Response> _repeateRequest(DioException e) {
    final opts = Options(method: e.requestOptions.method, headers: _dio.options.headers);

    return _dio.request(
      e.requestOptions.path,
      options: opts,
      data: e.requestOptions.data,
      queryParameters: e.requestOptions.queryParameters,
    );
  }
}

// class MultipartFileExtended extends MultipartFile {
//   final String? filePath;
//
//   MultipartFileExtended(
//     Stream<List<int>> stream,
//     length, {
//     filename,
//     this.filePath,
//     contentType,
//   }) : super(stream, length, filename: filename, contentType: contentType);
//
//   static MultipartFileExtended fromFileSync(
//     String filePath, {
//     String? filename,
//     MediaType? contentType,
//   }) =>
//       multipartFileFromPathSync(filePath, filename: filename, contentType: contentType);
// }

// MultipartFileExtended multipartFileFromPathSync(
//   String filePath, {
//   String? filename,
//   MediaType? contentType,
// }) {
//   filename ??= basename(filePath);
//   var file = File(filePath);
//   var length = file.lengthSync();
//   var stream = file.openRead();
//   return MultipartFileExtended(
//     stream,
//     length,
//     filename: filename,
//     contentType: contentType,
//     filePath: filePath,
//   );
// }

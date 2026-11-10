package com.qalaatal_dhaman.team

import android.content.Intent
import android.content.pm.PackageManager
import androidx.core.content.FileProvider
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel
import java.io.File

class MainActivity : FlutterActivity() {
    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)
        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, CHANNEL).setMethodCallHandler { call, result ->
            if (call.method == "printPng") {
                val path = call.argument<String>("path")
                if (path.isNullOrEmpty()) {
                    result.error("NO_PATH", "Missing image path", null)
                    return@setMethodCallHandler
                }
                try {
                    printPng(path)
                    result.success(true)
                } catch (e: Exception) {
                    result.error("PRINT_FAIL", e.message, null)
                }
            } else {
                result.notImplemented()
            }
        }
    }

    private fun printPng(path: String) {
        val file = File(path)
        if (file.exists() == false) {
            throw IllegalStateException("Image file not found")
        }
        val uri = FileProvider.getUriForFile(
            this,
            "${applicationContext.packageName}.fileprovider",
            file,
        )
        val send = Intent(Intent.ACTION_SEND).apply {
            type = "image/png"
            putExtra(Intent.EXTRA_STREAM, uri)
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }
        if (isPackageInstalled(RAWBT_PACKAGE)) {
            send.setPackage(RAWBT_PACKAGE)
            grantUriPermission(RAWBT_PACKAGE, uri, Intent.FLAG_GRANT_READ_URI_PERMISSION)
            startActivity(send)
            return
        }
        startActivity(Intent.createChooser(send, "طباعة الوصل"))
    }

    private fun isPackageInstalled(packageName: String): Boolean {
        return try {
            packageManager.getPackageInfo(packageName, 0)
            true
        } catch (_: PackageManager.NameNotFoundException) {
            false
        }
    }

    companion object {
        private const val CHANNEL = "com.qalaatal_dhaman.team/thermal_print"
        private const val RAWBT_PACKAGE = "ru.a402d.rawbtprinter"
    }
}

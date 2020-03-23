@file:Suppress("BlockingMethodInNonBlockingContext")

package com.companyname.weather.services

import android.content.Context
import androidx.lifecycle.MutableLiveData
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import java.io.File
import java.io.FileOutputStream
import java.net.URL

class FileCachingService   {

    fun getCachedFile(link: String, context: Context): MutableLiveData<String> {
        val data = MutableLiveData<String>()

        GlobalScope.launch {
            val url = URL(link)
            val fileName = url.file
            val cacheDir = context.cacheDir
            val cacheFile = File(cacheDir, fileName)

            if (!cacheFile.exists()) {
                cacheFile.parent?.let { File(it).mkdirs() }

                URL(link).openStream().use { input ->
                    FileOutputStream(cacheFile).use { output ->
                        input.copyTo(output)
                    }
                }
            }
            data.postValue(cacheFile.path)
        }

        return data
    }

}
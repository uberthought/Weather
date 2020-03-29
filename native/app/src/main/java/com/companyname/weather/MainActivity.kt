package com.companyname.weather

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.view.inputmethod.InputMethodManager
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.Toolbar
import androidx.drawerlayout.widget.DrawerLayout
import androidx.navigation.findNavController
import androidx.navigation.ui.AppBarConfiguration
import androidx.navigation.ui.navigateUp
import androidx.navigation.ui.setupActionBarWithNavController
import androidx.navigation.ui.setupWithNavController
import com.companyname.weather.services.LocationService
import com.companyname.weather.services.NWSService
import com.companyname.weather.services.PreferenceService
import com.google.android.material.navigation.NavigationView

class MainActivity : AppCompatActivity() {
    companion object {
        var instance: MainActivity? = null
        const val LOCATION_REQUEST: Int = 1
    }

    private lateinit var appBarConfiguration: AppBarConfiguration

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        instance = this

        setContentView(R.layout.activity_main)

        val toolbar: Toolbar = findViewById(R.id.toolbar)
        setSupportActionBar(toolbar)

        val drawerLayout: DrawerLayout = findViewById(R.id.drawer_layout)
        val navView: NavigationView = findViewById(R.id.nav_view)
        val navController = findNavController(R.id.nav_host_fragment)
        // Passing each menu ID as a set of Ids because each
        // menu should be considered as top level destinations.
        appBarConfiguration = AppBarConfiguration(setOf(
            R.id.nav_forecast, R.id.nav_settings), drawerLayout)
        setupActionBarWithNavController(navController, appBarConfiguration)
        navView.setupWithNavController(navController)

        val drawListener = object: DrawerLayout.DrawerListener {
            override fun onDrawerStateChanged(newState: Int) { }
            override fun onDrawerSlide(drawerView: View, slideOffset: Float) { }
            override fun onDrawerClosed(drawerView: View) { }
            override fun onDrawerOpened(drawerView: View) {
                val manager = getSystemService(Activity.INPUT_METHOD_SERVICE) as InputMethodManager
                val currentFocus = currentFocus
                currentFocus?.let { manager.hideSoftInputFromWindow(it.windowToken, 0) }
            }

        }
        drawerLayout.addDrawerListener(drawListener)

        startService(Intent(this, PreferenceService::class.java))
        startService(Intent(this, LocationService::class.java))
        startService(Intent(this, NWSService::class.java))
    }

    override fun onDestroy() {
        super.onDestroy()
        stopService(Intent(this, PreferenceService::class.java))
        stopService(Intent(this, LocationService::class.java))
        stopService(Intent(this, NWSService::class.java))
    }

    override fun onSupportNavigateUp(): Boolean {
        val navController = findNavController(R.id.nav_host_fragment)
        return navController.navigateUp(appBarConfiguration) || super.onSupportNavigateUp()
    }

    override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<out String>, grantResults: IntArray) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        when (requestCode) {
            LOCATION_REQUEST -> {
                if (permissions.isNotEmpty()) {
                    if (grantResults.any { r -> r == -1 })
                        PreferenceService.useDevice.value = false
                    else
                        LocationService.instance?.requestLocationUpdates()
                }
            }
        }
    }
}

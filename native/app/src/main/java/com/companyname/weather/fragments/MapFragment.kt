package com.companyname.weather.fragments

import android.annotation.SuppressLint
import android.content.pm.PackageManager
import android.location.Geocoder
import android.location.Location
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.app.ActivityCompat
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.navigation.fragment.findNavController
import com.companyname.weather.R
import com.companyname.weather.databinding.MapFragmentBinding
import com.companyname.weather.services.LocationService
import com.companyname.weather.services.PreferenceService
import com.google.android.gms.common.api.Status
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.Marker
import com.google.android.gms.maps.model.MarkerOptions
import com.google.android.libraries.places.api.Places
import com.google.android.libraries.places.api.model.Place
import com.google.android.libraries.places.widget.AutocompleteSupportFragment
import com.google.android.libraries.places.widget.listener.PlaceSelectionListener

class MapFragment : Fragment(), GoogleMap.OnMapClickListener {

    private lateinit var map: GoogleMap
    private var marker: Marker? = null

    @SuppressLint("MissingPermission")
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        val binding = MapFragmentBinding.inflate(layoutInflater)

        val autocompleteFragment = childFragmentManager.findFragmentById(R.id.autocomplete_fragment) as AutocompleteSupportFragment
        setupSearch(autocompleteFragment)

        val mapFragment = childFragmentManager.findFragmentById(R.id.google_map_fragment) as SupportMapFragment
        mapFragment.getMapAsync { setupMap(it) }

        binding.useDeviceLocation.setOnClickListener { PreferenceService.useDevice.value = true }

        LocationService.hasPermission.observe(viewLifecycleOwner, Observer { onUseDeviceChanged() })
        PreferenceService.useDevice.observe(viewLifecycleOwner, Observer { onUseDeviceChanged() })

        return binding.root
    }

    private var dismissing = false

    private fun onUseDeviceChanged() {
        if (LocationService.hasPermission.value == true && PreferenceService.useDevice.value == true && !dismissing) {
            dismissing = true
            findNavController().popBackStack()
        }
    }

    private fun setupSearch(autocompleteFragment: AutocompleteSupportFragment) {
        if (!Places.isInitialized())
            context?.let { context ->
                val mapKey = context.packageManager.getApplicationInfo(context.packageName, PackageManager.GET_META_DATA).metaData.getString("com.google.android.geo.API_KEY")
                mapKey?.let { Places.initialize(context, it) }
            }

        autocompleteFragment.setPlaceFields(listOf(Place.Field.LAT_LNG, Place.Field.ADDRESS))
        val placeSelectionListener = object : PlaceSelectionListener {
            override fun onPlaceSelected(place: Place) {
                place.latLng?.let {
                    PreferenceService.location.value = with (Location("")) {
                        latitude = it.latitude
                        longitude = it.longitude
                        this
                    }
                }
                place.address?.let { PreferenceService.locationName.value = it }
            }

            override fun onError(status: Status) {}
        }
        autocompleteFragment.setOnPlaceSelectedListener(placeSelectionListener)
    }

    private fun setupMap(map: GoogleMap) {
        this.map = map
        map.uiSettings.isZoomControlsEnabled = true
        if (LocationService.granularity.all { l -> ActivityCompat.checkSelfPermission(context!!, l) == PackageManager.PERMISSION_GRANTED })
            map.isMyLocationEnabled = true
        map.setOnMapClickListener(this)

        LocationService.location.value?.let {
            val latLng = LatLng(it.latitude, it.longitude)
            map.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 9f))
        }
        LocationService.location.observe(viewLifecycleOwner, Observer { moveToLocation(it) })
    }

    private fun moveToLocation(location: Location) {
        if (location.latitude == 0.0 && location.longitude == 0.0) {
            val latLng = LatLng(36.0, -95.5)
            map.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 3.1f))
        }
        else {
            val latLng = LatLng(location.latitude, location.longitude)
            map.animateCamera(CameraUpdateFactory.newLatLng(latLng))
            updateMarker(latLng)
        }
    }

    override fun onMapClick(latLng: LatLng?) {
        latLng ?: return

        updateMarker(latLng)

        map.animateCamera(CameraUpdateFactory.newLatLng(latLng))

        PreferenceService.location.value = with (Location("")) {
            latitude = latLng.latitude
            longitude = latLng.longitude
            this
        }

        val geocoder = Geocoder(context!!)
        val addresses = geocoder.getFromLocation(latLng.latitude, latLng.longitude, 1)
        PreferenceService.locationName.value =
            if (addresses.count() == 0) "${latLng.latitude}, ${latLng.longitude}"
            else addresses[0].getAddressLine(0)
    }

    private fun updateMarker(latLng: LatLng) {
        if (marker != null)
            marker?.position = latLng
        else
            marker = map.addMarker(MarkerOptions().position(latLng).title("Forecast Location"))
    }

}
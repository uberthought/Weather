package com.companyname.weather.fragments

import android.graphics.BitmapFactory
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.constraintlayout.widget.ConstraintLayout
import androidx.constraintlayout.widget.ConstraintSet
import androidx.fragment.app.Fragment
import androidx.fragment.app.FragmentContainerView
import androidx.lifecycle.Observer
import androidx.lifecycle.ViewModelProvider
import com.companyname.weather.viewModels.ConditionsViewModel
import com.companyname.weather.viewModels.DetailedConditionsViewModel
import com.companyname.weather.R
import com.companyname.weather.services.FileCachingService
import com.companyname.weather.databinding.DetailedConditionsFragmentBinding
import kotlinx.android.synthetic.main.detailed_conditions_fragment.*
import kotlinx.android.synthetic.main.detailed_conditions_fragment.view.*

class DetailedConditionsFragment : Fragment() {
    private lateinit var viewModel: DetailedConditionsViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        viewModel = ViewModelProvider(this)[DetailedConditionsViewModel::class.java]

        val binding = DetailedConditionsFragmentBinding.inflate(layoutInflater)
        binding.lifecycleOwner = this
        binding.viewModel = viewModel
        binding.fragment = this

        // if the view model isn't set, start with current conditions
        if (tag == "LandscapeDetailedConditionsFragment") {
            val conditionsViewModel = ViewModelProvider(this)[ConditionsViewModel::class.java]
            conditionsViewModel.details.observe(viewLifecycleOwner, Observer {
                viewModel.details.postValue(conditionsViewModel.details.value)
                conditionsViewModel.details.removeObservers(viewLifecycleOwner)
            })
        }

        viewModel.details.observe(viewLifecycleOwner, Observer {
            binding.invalidateAll()

            val iconUrl = viewModel.details.value?.icon?.replace("medium", resources.getString(R.string.smallIconSize))
            iconUrl?.let {
                FileCachingService.instance.getCachedFile(it, context).observe(viewLifecycleOwner, Observer { path ->
                    val bitmap = BitmapFactory.decodeFile(path)
                    binding.icon.setImageBitmap(bitmap)
                })
            }
        })

        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        val containerTag = (view.parent as? FragmentContainerView)?.tag
        if (containerTag == "LandscapeDetailedConditionsFragment") {
            conditionsCard.layoutParams.width = ViewGroup.LayoutParams.MATCH_PARENT
            conditionsCard.layoutParams.height = ViewGroup.LayoutParams.MATCH_PARENT
        }
    }

    fun onClick(view:View) {
        if (tag == "PortraitDetailedConditionsFragment") {
            parentFragmentManager.beginTransaction()
                .remove(this)
                .commit()
        }
    }
}

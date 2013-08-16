function adminicaWizard() {
	$(".wizard_progressbar").progressbar({ value: 10 });

	$('.wizard_steps ul li a').bind('click', function () {
	    var wizard = $(this).parents('.wizard');
	    var steps = $(this).parents('.wizard_steps');
	    var stepNext = steps.find('li').index($(this).parent()) + 1;
	    var stepCurrent = steps.find('li').index(steps.find('li.current')) + 1;

		//console.log("step clicked: "+stepNext);
		//console.log("step current: " + stepCurrent);
		if (stepCurrent > stepNext || stepNext >= stepCurrent+2) {
		    return false;
		    $(wizard).find('label.error').css("display", "none");
		}
		else{
		    $(wizard).find(".validate_form").valid();
		}


		var errorsPresent = $(wizard).find('.step label.error').filter(":visible").length;
		console.log(errorsPresent);

		if (errorsPresent < 1) {
		    var nextButton = $(wizard).find('.wizard_content').find('.step[data-step=\'' + stepCurrent + '\'] button.next_step');
		    if ($(nextButton).data('next')) {
		        var retVal = $(nextButton).data('next')();
		        if (!retVal)
		            return false;
		    }
		    
			$('.wizard_steps ul li').removeClass('current');
			$(this).parent('li').addClass('current');

			var step_multiplyby = (100 / steps.find('li').size());
			var prog_val = (stepNext*step_multiplyby);

			$(wizard).find(".wizard_progressbar").progressbar({ value: prog_val });

			$(wizard).find('.wizard_content').find('.step').hide();
			$(wizard).find('.wizard_content').find('.step[data-step=\'' + stepNext + '\']').fadeIn(1000);
			columnHeight();
		}
		return false;
	});

	var initialProg = (100 / $(".wizard_steps > ul > li").size());

	$(".wizard_progressbar").progressbar({value : initialProg});

	$('.wizard .button_bar button.next_step').bind('click', function () {
	    var steps = $(this).parents('.wizard').find('.wizard_steps');
	    var nextStep = steps.find('li').index(steps.find('li.current')) + 2;

	    $(this).parents('.wizard').find('.wizard_steps ul li:nth-child(' + nextStep + ') a').trigger('click');
	});
    
    /*
	$('.wizard_content form .submit_button').bind('click', function(){
		$(".validate_form").valid();

		var errorsPresent = $(this).parents('form').find('.error').html();

		if (errorsPresent) {
		    $(this).parents('form').submit();
		}
		else{
		    console.log("error");
		}
	});
    */

	$(".validate_form").validate();
}
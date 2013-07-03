function adminicaWizard() {
    
	$(".wizard_progressbar").progressbar({ value: 10 });

	$('.wizard_steps ul li a').on('click', function () {
	    var steps = $(this).parents('.wizard_steps');
	    var step_num = steps.find('li').index($(this).parent()) + 1;
	    var step_current = steps.find('li').index(steps.find('li.current')) + 1;

		console.log("step clicked: "+step_num);
		console.log("step current: "+step_current);
		if (step_current > step_num){
			$('label.error').css("display","none");
		}
		else{
			$(".validate_form").valid();
		}


		var errorsPresent = $('.step').find('label.error').filter(":visible").length;
		console.log(errorsPresent);

		if(errorsPresent < 1){
			$('.wizard_steps ul li').removeClass('current');
			$(this).parent('li').addClass('current');

			var step = $(this).attr('href');
			var step_multiplyby = (100 / steps.find('li').size());
			var prog_val = (step_num*step_multiplyby);

			$( ".wizard_progressbar").progressbar({ value: prog_val });

			$('.wizard_content').find('.step').hide();
			$('.wizard_content').find(step).fadeIn(1000);
		}
		return false;
	});

	var initialProg = (100 / $(".wizard_steps > ul > li").size());

	$( ".wizard_progressbar").progressbar({ value : initialProg});

	$('.wizard .button_bar button:not(".submit_button")').on('click', function() {

	    //if (!$(this).attr('data-goto'))
	    //    return;

	    //var goTo = $(this).attr("data-goto").replace('step_','');
	    

		//$('.wizard_steps ul li:nth-child('+goTo+') a').trigger('click');

		columnHeight();
	});

	$('.wizard_content form .submit_button').on('click', function(){
		$(".validate_form").valid();

		var errorsPresent = $(this).parents('form').find('.error').html();

		if (errorsPresent) {
		    $(this).parents('form').submit();
		}
		else{
		    console.log("error");
		}
	});

	$(".validate_form").validate();
}
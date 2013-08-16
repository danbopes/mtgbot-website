function adminicaGallery(){



	// Delete Button

	$(".delete_buttons li").addClass("delete_able").append('<div class="delete ui-icon ui-icon-trash dialog_button" data-dialog="dialog_delete"></div>');

	$(".delete").on("click", function(){
		$(".delete_able").removeClass("delete_cue");
		$(this).parents(".delete_able").addClass("delete_cue");
	});

	$(".delete_confirm").on("click", function(){
		$(".delete_cue").fadeOut("fast", function(){
			$(this).remove();
			$(".isotope_holder ul").isotope( 'remove', $(this) );
			refreshIsotope();
		});
	});



	// Fancy Box 2

	if($.fn.fancybox){
		$(".gallery.fancybox li a").fancybox({
        	'overlayColor':'#000'
		});

		$("a img.fancy").fancybox();

		$("a.fancybox_media").fancybox({
			openEffect  : 'none',
			closeEffect : 'none',
			helpers : {
				media : {}
			}
		});
	}
}
/*
$(document).ready(function() {
	$("h2").click(function() {
		$(".customer").slideToggle();
	});
});

$(".accod-content").css("background-color", "#96c8da");


*/

$(document).ready(function() {

	$(".accod-head").click(function() {
		$(".accod-content").hide().prev(".accod-head").removeClass("accod-head-select");
		$(this).next(".accod-content").show().prev(".accod-head").addClass("accod-head-select");
	});
	
	$(".customer-head").click(function() {
		$(".customer-content").slideToggle();
	});

	$(".customerUpload-head").click(function () {
	    $(".customerUpload-content").slideToggle();
    });
	
	$(".work-order-details-head").click(function () {
		$(".work-order-details-content").slideToggle();
	});

	$(".work-requested-details-head").click(function () {
	    $(".work-requested-details-content").slideToggle();
	});

	$(".parts-ordering-details-head").click(function () {
	    $(".parts-ordering-details-content").slideToggle();
	});

	$(".erf-details-head").click(function () {
	    $(".erf-details-content").slideToggle();
	});

	$(".notes-details-head").click(function () {
	    $(".notes-details-content").slideToggle();
	});

	$(".work-order-dispatch-head").click(function () {
	    $(".work-order-dispatch-content").slideToggle();
	});

	$(".closure-head").click(function () {
	    $(".closure-content").slideToggle();
	});

	$(".know-equipment-details-head").click(function () {
	    $(".know-equipment-details-content").slideToggle();
	});
	
	$(".work-performed-head").click(function() {
		$(".work-performed-content").slideToggle();
	});
	
	$(".parts-ordering-head").click(function() {
		$(".parts-ordering-content").slideToggle();
	});
	
	$(".load-van-head").click(function() {
		$(".load-van-content").slideToggle();
	});
	
	$(".known-equipment-head").click(function() {
		$(".known-equipment-content").slideToggle();
	});
	
	$(".work-order-history-head").click(function() {
		$(".work-order-history-content").slideToggle();
	});

	$(".equipment-head").click(function () {
	    $(".equipment-details-content").slideToggle();
	});
	
	$(".expandible-head").click(function () {
	    $(".expandible-details-content").slideToggle();
	});

	$(".contingent-head").click(function () {
	    $(".contingent-content").slideToggle();
	});

	$(".contingentItem-head").click(function () {
	    $(".contingentItem-content").slideToggle();
	});

	$(".orderTypeItem-head").click(function () {
	    $(".orderTypeItem-content").slideToggle();
	});

	 $(".branchItem-head").click(function () {
	     $(".branchItem-content").slideToggle();
	 });

	 $(".eqp-summary-head").click(function () {
	     $(".eqp-summary-content").slideToggle();
	 });

	 $(".branchPSP-head").click(function () {
	     $(".branchPSP-content").slideToggle();
	 });

	 $(".bulkERF-head").click(function () {
	     $(".bulkERF-content").slideToggle();
	 });
});







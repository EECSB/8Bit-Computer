// Copyright (C) 1991-2013 Altera Corporation
// Your use of Altera Corporation's design tools, logic functions 
// and other software and tools, and its AMPP partner logic 
// functions, and any output files from any of the foregoing 
// (including device programming or simulation files), and any 
// associated documentation or information are expressly subject 
// to the terms and conditions of the Altera Program License 
// Subscription Agreement, Altera MegaCore Function License 
// Agreement, or other applicable license agreement, including, 
// without limitation, that your use is for the sole purpose of 
// programming logic devices manufactured by Altera and sold by 
// Altera or its authorized distributors.  Please refer to the 
// applicable agreement for further details.

// PROGRAM		"Quartus II 64-Bit"
// VERSION		"Version 13.0.1 Build 232 06/12/2013 Service Pack 1 SJ Web Edition"
// CREATED		"Mon Jul 02 18:34:02 2018"



module DFF_Edge_Trigger(
	CLK,
	D,
	CLR,
	Q,
	NQ
);


input wire	CLK;
input wire	D;
input wire	CLR;
output wire	Q;
output wire	NQ;

wire	SYNTHESIZED_WIRE_0;
wire	SYNTHESIZED_WIRE_10;
wire	SYNTHESIZED_WIRE_11;
wire	SYNTHESIZED_WIRE_12;
wire	SYNTHESIZED_WIRE_6;
wire	SYNTHESIZED_WIRE_7;

assign	Q = SYNTHESIZED_WIRE_0;
assign	NQ = SYNTHESIZED_WIRE_7;



assign	SYNTHESIZED_WIRE_7 = ~(SYNTHESIZED_WIRE_0 & SYNTHESIZED_WIRE_10 & CLR);

assign	SYNTHESIZED_WIRE_10 = ~(SYNTHESIZED_WIRE_11 & CLK & SYNTHESIZED_WIRE_12);

assign	SYNTHESIZED_WIRE_6 = ~(SYNTHESIZED_WIRE_11 & SYNTHESIZED_WIRE_12);

assign	SYNTHESIZED_WIRE_11 = ~(CLK & SYNTHESIZED_WIRE_6);

assign	SYNTHESIZED_WIRE_0 = ~(SYNTHESIZED_WIRE_7 & SYNTHESIZED_WIRE_11);

assign	SYNTHESIZED_WIRE_12 = ~(D & SYNTHESIZED_WIRE_10);


endmodule

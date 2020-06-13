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
// CREATED		"Fri Aug 17 00:15:01 2018"

module DFF_Master_Slave(
	D,
	C,
	Q,
	NQ
);


input wire	D;
input wire	C;
output wire	Q;
output wire	NQ;

wire	SYNTHESIZED_WIRE_0;
wire	SYNTHESIZED_WIRE_1;
wire	SYNTHESIZED_WIRE_2;
wire	SYNTHESIZED_WIRE_3;
wire	SYNTHESIZED_WIRE_17;
wire	SYNTHESIZED_WIRE_18;
wire	SYNTHESIZED_WIRE_19;
wire	SYNTHESIZED_WIRE_8;
wire	SYNTHESIZED_WIRE_10;
wire	SYNTHESIZED_WIRE_11;
wire	SYNTHESIZED_WIRE_12;
wire	SYNTHESIZED_WIRE_15;

assign	Q = SYNTHESIZED_WIRE_3;
assign	NQ = SYNTHESIZED_WIRE_0;



assign	SYNTHESIZED_WIRE_3 = ~(SYNTHESIZED_WIRE_0 & SYNTHESIZED_WIRE_1);

assign	SYNTHESIZED_WIRE_0 = ~(SYNTHESIZED_WIRE_2 & SYNTHESIZED_WIRE_3);

assign	SYNTHESIZED_WIRE_17 =  ~C;

assign	SYNTHESIZED_WIRE_18 =  ~SYNTHESIZED_WIRE_17;

assign	SYNTHESIZED_WIRE_1 = ~(SYNTHESIZED_WIRE_18 & SYNTHESIZED_WIRE_19);

assign	SYNTHESIZED_WIRE_2 = ~(SYNTHESIZED_WIRE_18 & SYNTHESIZED_WIRE_8);

assign	SYNTHESIZED_WIRE_8 =  ~SYNTHESIZED_WIRE_19;

assign	SYNTHESIZED_WIRE_19 = ~(SYNTHESIZED_WIRE_10 & SYNTHESIZED_WIRE_11);

assign	SYNTHESIZED_WIRE_10 = ~(SYNTHESIZED_WIRE_12 & SYNTHESIZED_WIRE_19);

assign	SYNTHESIZED_WIRE_12 = ~(SYNTHESIZED_WIRE_17 & SYNTHESIZED_WIRE_15);

assign	SYNTHESIZED_WIRE_11 = ~(SYNTHESIZED_WIRE_17 & D);

assign	SYNTHESIZED_WIRE_15 =  ~D;


endmodule

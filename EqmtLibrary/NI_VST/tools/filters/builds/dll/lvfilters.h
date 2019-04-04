#include "extcode.h"
#ifdef __cplusplus
extern "C" {
#endif

/*!
 * Initialize
 */
void __cdecl Initialize(void);
/*!
 * ComplexFilter
 */
void __cdecl ComplexFilter(double iqRate, double vbw, 
	int32_t leadingFirstPointSamples, double bandwidth, double minSelectivity, 
	double real[], double imaginary[], double FilteredData[], int32_t len);

MgErr __cdecl LVDLLStatus(char *errStr, int errStrLen, void *module);

#ifdef __cplusplus
} // extern "C"
#endif


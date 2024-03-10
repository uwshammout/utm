#ifndef CyclicTaskExecutor_h
#define CyclicTaskExecutor_h


namespace cblk {
  
  typedef void(*exec_func_t)();
  
  
  class CyclicTaskExecutor {
    private:
    
    exec_func_t funcPtr;
    unsigned int callingInterval_ms;
    unsigned long lastCalledTime_ms;
  
    
    public:
    
    CyclicTaskExecutor(exec_func_t func_ptr, unsigned int calling_interval_ms, bool call_on_zero = false);
    void change_interval(unsigned int calling_interval_ms, bool call_on_zero = false);
    void update(unsigned long current_time_ms);
  };
  
}


#endif
